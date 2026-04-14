using AutoMapper;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.ExternalServices.AI;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Repositories;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SmartSearchCoach : ISmartSearchCoach
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStoreService _vectorStoreService;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly ISmartSearchReasoningService _reasoningService;
        private readonly IMapper _mapper;

        public SmartSearchCoach(
            IEmbeddingService embeddingService,
            IVectorStoreService vectorStoreService,
            ICoachProfileRepository coachProfileRepository,
            ISmartSearchReasoningService reasoningService,
            IMapper mapper)
        {
            _embeddingService = embeddingService;
            _vectorStoreService = vectorStoreService;
            _coachProfileRepository = coachProfileRepository;
            _reasoningService = reasoningService;
            _mapper = mapper;
        }

        public async Task<List<SmartSearchResultDto>> ExecuteAsync(SmartSearchRequest request)
        {
            const int reasoningCandidateTopK = 5;
            const int finalOutputTopN = 3;

            if (string.IsNullOrWhiteSpace(request.Query) && string.IsNullOrWhiteSpace(request.ExtractedProfileContext))
                throw new ArgumentException("Search query and profile context cannot both be empty.");

            // Step 1: Distill context + embed (sequential — embed depends on distilled text)
            string searchContext = DistillSearchContext(request.Query, request.ExtractedProfileContext);
            var queryVector = await _embeddingService.GetEmbeddingAsync(searchContext, "query");

            // Step 2: Pinecone vector search
            var vectorTopK = Math.Max(request.TopK, reasoningCandidateTopK);
            var metadataFilters = BuildMetadataFilters(request.ExtractedProfileContext);
            var vectorMatches = await _vectorStoreService.SearchAsync(
                queryVector, vectorTopK, request.Namespace, metadataFilters);

            if (!vectorMatches.Any())
                return new List<SmartSearchResultDto>();

            // Collect valid coach IDs + their vector scores from Pinecone results
            var validMatches = new List<(Guid CoachId, double Score)>();
            foreach (var match in vectorMatches)
            {
                if (!IsValidCoachMatch(match)) continue;
                if (Guid.TryParse(match.Id, out var coachId))
                    validMatches.Add((coachId, match.Score));
            }

            if (!validMatches.Any())
                return new List<SmartSearchResultDto>();

            // Step 3: Batch DB query + LLM rerank IN PARALLEL
            // These are independent: DB fetches profile data, LLM scores based on Pinecone metadata.
            // We build LLM candidates from Pinecone metadata (already available) to avoid waiting for DB.
            var reasoningCandidates = BuildReasoningCandidatesFromMetadata(vectorMatches, validMatches);

            var dbTask = _coachProfileRepository.GetProfilesByIdsAsync(validMatches.Select(m => m.CoachId));
            var llmTask = reasoningCandidates.Any()
                ? _reasoningService.RerankAndReasonAsync(searchContext, reasoningCandidates)
                : Task.FromResult(new List<ReasoningResult>());

            await Task.WhenAll(dbTask, llmTask);

            var coachProfiles = await dbTask;
            var reasoningResults = await llmTask;

            // Step 4: Build result DTOs from DB data
            var profileMap = coachProfiles.ToDictionary(p => p.Id);
            var scoreMap = validMatches.ToDictionary(m => m.CoachId, m => m.Score);
            var results = new List<SmartSearchResultDto>();

            foreach (var (coachId, vectorScore) in validMatches)
            {
                if (!profileMap.TryGetValue(coachId, out var coachProfile)) continue;

                var topSkills = coachProfile.Skills?.Take(3).Select(s => s.Name).ToList() ?? new List<string>();
                var companies = coachProfile.Companies?.Select(c => new CompanySummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    LogoPath = c.LogoPath
                }).ToList() ?? new List<CompanySummaryDto>();

                results.Add(new SmartSearchResultDto
                {
                    CoachId = coachId,
                    FullName = coachProfile.User?.FullName ?? "Unknown User",
                    ProfilePicture = coachProfile.User?.ProfilePicture,
                    SlugProfileUrl = coachProfile.User?.SlugProfileUrl,
                    ExperienceYears = coachProfile.ExperienceYears ?? 0,
                    PortfolioUrl = coachProfile.PortfolioUrl,
                    TopSkills = topSkills,
                    Companies = companies,
                    MatchScore = vectorScore,
                    FinalScore = vectorScore,
                    RerankSource = "Pinecone"
                });
            }

            // Step 5: Apply AI re-ranking scores
            if (reasoningResults.Any())
            {
                var reasoningMap = new Dictionary<string, ReasoningResult>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in reasoningResults)
                {
                    var normalizedId = r.Id?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(normalizedId) && !reasoningMap.ContainsKey(normalizedId))
                        reasoningMap[normalizedId] = r;
                }

                foreach (var result in results)
                {
                    var coachIdStr = result.CoachId.ToString();
                    if (reasoningMap.TryGetValue(coachIdStr, out var aiResult))
                    {
                        result.RerankScore = aiResult.Score;
                        result.FinalScore = aiResult.Score;
                        result.Reasoning = aiResult.Reasoning;
                        result.RerankSource = "AI";
                    }
                    else
                    {
                        result.RerankScore = result.MatchScore * 0.5;
                        result.FinalScore = result.MatchScore * 0.5;
                        result.RerankSource = "Pinecone-Fallback";

                        var skills = result.TopSkills != null && result.TopSkills.Any()
                            ? string.Join(", ", result.TopSkills)
                            : "general coaching";
                        var exp = result.ExperienceYears > 0
                            ? $"{result.ExperienceYears} years of experience"
                            : "relevant experience";
                        result.Reasoning = $"Matched by profile similarity. This coach brings {exp} in {skills}, which may help strengthen your preparation for the target role.";
                    }
                }
            }

            return results
                .OrderByDescending(r => r.FinalScore)
                .Take(finalOutputTopN)
                .ToList();
        }

        /// <summary>
        /// Builds LLM reasoning candidates from Pinecone metadata (already available after vector search)
        /// so we can fire the LLM call without waiting for the DB query.
        /// </summary>
        private static List<ReasoningCandidate> BuildReasoningCandidatesFromMetadata(
            List<VectorMatch> vectorMatches,
            List<(Guid CoachId, double Score)> validMatches)
        {
            var validIds = validMatches.Select(m => m.CoachId.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var candidates = new List<ReasoningCandidate>();

            foreach (var match in vectorMatches)
            {
                if (!validIds.Contains(match.Id)) continue;
                if (match.Metadata == null) continue;

                match.Metadata.TryGetValue("name", out var name);
                match.Metadata.TryGetValue("bio", out var bio);
                match.Metadata.TryGetValue("skills", out var skills);
                match.Metadata.TryGetValue("experienceYears", out var yoe);
                match.Metadata.TryGetValue("currentJobTitle", out var jobTitle);

                candidates.Add(new ReasoningCandidate
                {
                    Id = match.Id,
                    Summary = $"Name: {name}. Title: {jobTitle}. Experience: {yoe} yrs. Bio: {bio}. Skills: {skills}."
                });
            }

            return candidates;
        }

        /// <summary>
        /// Builds Pinecone metadata filters from the CV/JD context.
        /// Supports $eq (string), $in (array match on skills), and $gte (numeric experienceYears).
        /// To disable context-based pre-filtering (e.g. for testing), set enableContextFilters = false.
        /// </summary>
        private static Dictionary<string, object> BuildMetadataFilters(string? extractedProfileContext)
        {
            var baseFilter = new Dictionary<string, object> { ["entityType"] = "coach" };

            // ── Toggle: set to true ONLY AFTER re-syncing coach vectors (SyncCoachVectors) ──
            // Old vectors store skills/industries as comma-separated strings — $in/$gte filters won't work.
            const bool enableContextFilters = false;
            if (!enableContextFilters)
                return baseFilter;

            if (string.IsNullOrWhiteSpace(extractedProfileContext))
                return baseFilter;

            try
            {
                var context = JsonNode.Parse(extractedProfileContext)?.AsObject();
                if (context == null) return baseFilter;

                var jd = context["jd"]?.AsObject();
                if (jd != null)
                {
                    // $in filter on skills — match coaches who have ANY of the JD's must-have skills.
                    // Pinecone $in on array metadata: returns vectors where the array contains at least one match.
                    var mustHaveSkills = jd["must_have_skills"]?.AsArray();
                    if (mustHaveSkills != null && mustHaveSkills.Count > 0)
                    {
                        var skillNames = mustHaveSkills
                            .Select(s => s?.GetValue<string>()?.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToArray();
                        if (skillNames.Length > 0)
                            baseFilter["skills"] = skillNames!;
                    }

                    // $gte filter on experienceYears — coaches must meet minimum YoE from JD.
                    var requiredYoe = jd["required_yoe"];
                    if (requiredYoe != null)
                    {
                        if (double.TryParse(requiredYoe.ToString(), out var yoe) && yoe > 0)
                            baseFilter["experienceYears"] = new NumericFilter(Gte: yoe);
                    }
                }

                return baseFilter;
            }
            catch
            {
                return baseFilter;
            }
        }

        /// <summary>
        /// Builds the search context string from the query and extracted profile.
        ///
        /// The frontend now sends a pre-distilled search profile (only search-critical fields)
        /// produced by the Python AI service's build_search_profile(). This method detects that
        /// case and passes the data through without further stripping.
        ///
        /// Legacy fallback: if the input still contains raw cv/jd wrappers with bloated fields
        /// (core_responsibilities, benefits, descriptions, etc.), it distills them server-side.
        /// </summary>
        private static string DistillSearchContext(string query, string? extractedProfileContext)
        {
            if (string.IsNullOrWhiteSpace(extractedProfileContext))
                return query;

            try
            {
                var context = JsonNode.Parse(extractedProfileContext)?.AsObject();
                if (context == null)
                    return $"Goal: {query}\nProfile: {extractedProfileContext}";

                // Detect pre-distilled input: if cv/jd sections lack bloated fields
                // (no core_responsibilities, benefits, company_culture, description in experiences)
                // then the data is already clean — strip empty fields and pass through.
                if (IsAlreadyDistilled(context))
                {
                    StripEmptyFields(context);
                    var compactStr = context.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
                    return $"Goal: {query}\nProfile: {compactStr}";
                }

                // Legacy path: raw CV/JD with all fields — distill server-side
                var distilled = new JsonObject();

                var jd = context["jd"]?.AsObject();
                if (jd != null)
                {
                    if (jd["job_title"] != null) distilled["targetRole"] = JsonNode.Parse(jd["job_title"]!.ToJsonString());
                    if (jd["must_have_skills"] != null) distilled["requiredSkills"] = JsonNode.Parse(jd["must_have_skills"]!.ToJsonString());
                    if (jd["nice_to_have_skills"] != null) distilled["bonusSkills"] = JsonNode.Parse(jd["nice_to_have_skills"]!.ToJsonString());
                    if (jd["required_yoe"] != null) distilled["requiredYoe"] = JsonNode.Parse(jd["required_yoe"]!.ToJsonString());
                }

                var cv = context["cv"]?.AsObject();
                if (cv != null)
                {
                    if (cv["skills"] is JsonArray skillArr && skillArr.Count > 0)
                        distilled["candidateSkills"] = JsonNode.Parse(skillArr.ToJsonString());
                    if (cv["total_years_of_experience"] != null) distilled["candidateYoe"] = JsonNode.Parse(cv["total_years_of_experience"]!.ToJsonString());

                    var experiences = cv["experiences"]?.AsArray();
                    if (experiences != null && experiences.Count > 0)
                    {
                        var pruned = new JsonArray();
                        foreach (var exp in experiences)
                        {
                            if (exp == null) continue;
                            var slim = new JsonObject();
                            if (exp["company"] != null) slim["company"] = JsonNode.Parse(exp["company"]!.ToJsonString());
                            if (exp["title"] != null) slim["title"] = JsonNode.Parse(exp["title"]!.ToJsonString());
                            pruned.Add(slim);
                        }
                        if (pruned.Count > 0) distilled["candidateExp"] = pruned;
                    }
                    if (cv["apply_for"] != null && distilled["targetRole"] == null)
                        distilled["targetRole"] = JsonNode.Parse(cv["apply_for"]!.ToJsonString());
                }

                var distilledStr = distilled.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
                return $"Goal: {query}\nProfile: {distilledStr}";
            }
            catch
            {
                return $"Goal: {query}\nProfile: {extractedProfileContext}";
            }
        }

        /// <summary>
        /// Checks if the profile context has already been distilled by the Python AI service.
        /// Pre-distilled profiles lack bloated fields like core_responsibilities, benefits,
        /// company_culture (JD) and description/dates in experiences (CV).
        /// </summary>
        private static bool IsAlreadyDistilled(JsonObject context)
        {
            var jd = context["jd"]?.AsObject();
            if (jd != null)
            {
                // Raw JD has these fields; distilled does not
                if (jd["core_responsibilities"] != null || jd["benefits"] != null || jd["company_culture"] != null)
                    return false;
            }

            var cv = context["cv"]?.AsObject();
            if (cv != null)
            {
                // Raw CV has languages/certifications; distilled does not
                if (cv["languages"] != null || cv["certifications"] != null)
                    return false;

                // Check if experiences still have description fields
                var experiences = cv["experiences"]?.AsArray();
                if (experiences != null)
                {
                    foreach (var exp in experiences)
                    {
                        if (exp?["description"] != null || exp?["dates"] != null)
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Recursively removes empty arrays, empty strings, and null values from a JsonObject
        /// so they don't pollute the embedding text or LLM prompt.
        /// </summary>
        private static void StripEmptyFields(JsonObject obj)
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in obj)
            {
                var node = kvp.Value;
                if (node == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
                else if (node is JsonArray arr && arr.Count == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
                else if (node is JsonValue val && val.TryGetValue<string>(out var str) && string.IsNullOrWhiteSpace(str))
                {
                    keysToRemove.Add(kvp.Key);
                }
                else if (node is JsonObject child)
                {
                    StripEmptyFields(child);
                    if (child.Count == 0) keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
                obj.Remove(key);
        }

        private static bool IsValidCoachMatch(VectorMatch match)
        {
            // Guard against cross-entity or mismatched metadata.
            if (match.Metadata == null)
            {
                return false;
            }

            if (!match.Metadata.TryGetValue("entityType", out var entityType) ||
                !string.Equals(entityType, "coach", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (match.Metadata.TryGetValue("entityId", out var entityId) &&
                Guid.TryParse(entityId, out var parsedEntityId) &&
                Guid.TryParse(match.Id, out var parsedMatchId))
            {
                return parsedEntityId == parsedMatchId;
            }

            return true;
        }
    }
}
