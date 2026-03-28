using AutoMapper;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.ExternalServices.AI;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Repositories;

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
            if (string.IsNullOrWhiteSpace(request.Query) && string.IsNullOrWhiteSpace(request.ExtractedProfileContext))
                throw new ArgumentException("Search query and profile context cannot both be empty.");

            // Combine the natural language query and structured JSON context to maximize vector search accuracy
            string searchContext = request.Query;
            if (!string.IsNullOrWhiteSpace(request.ExtractedProfileContext))
            {
                searchContext = $"Query: {request.Query}\nContext: {request.ExtractedProfileContext}";
            }

            // Use query embedding mode for the combined text context.
            var queryVector = await _embeddingService.GetEmbeddingAsync(searchContext, "query");

            // Search only in coach vectors.
            var vectorMatches = await _vectorStoreService.SearchAsync(
                queryVector,
                request.TopK,
                request.Namespace,
                new Dictionary<string, string> { ["entityType"] = "coach" });

            if (!vectorMatches.Any())
                return new List<SmartSearchResultDto>();

            var results = new List<SmartSearchResultDto>();
            var reasoningCandidates = new List<ReasoningCandidate>();

            foreach (var match in vectorMatches)
            {
                if (!IsValidCoachMatch(match)) continue;

                if (Guid.TryParse(match.Id, out var coachId))
                {
                    var coachProfile = await _coachProfileRepository.GetProfileByIdAsync(coachId);
                    if (coachProfile != null)
                    {
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
                            MatchScore = match.Score,
                            FinalScore = match.Score, // Initially fallback to vector score
                            RerankSource = "Pinecone"
                        });

                        // Prepare concise summary for LLM to avoid token bloat
                        var skills = string.Join(", ", topSkills);
                        var roles = coachProfile.Bio ?? "";
                        reasoningCandidates.Add(new ReasoningCandidate
                        {
                            Id = coachId.ToString(),
                            Summary = $"Portfolio: {coachProfile.PortfolioUrl}. Experience: {coachProfile.ExperienceYears ?? 0} yrs. Bio: {roles}. Skills: {skills}."
                        });
                    }
                }
            }

            // Step 4: AI Reasoning & Re-ranking
            if (reasoningCandidates.Any())
            {
                var reasoningResults = await _reasoningService.RerankAndReasonAsync(searchContext, reasoningCandidates);

                if (reasoningResults.Any())
                {
                    // Apply new scores and reasoning
                    var reasoningMap = reasoningResults.ToDictionary(r => r.Id, r => r);
                    foreach (var result in results)
                    {
                        if (reasoningMap.TryGetValue(result.CoachId.ToString(), out var aiResult))
                        {
                            result.RerankScore = aiResult.Score;
                            result.FinalScore = aiResult.Score; // AI score overrides final score
                            result.Reasoning = aiResult.Reasoning;
                            result.RerankSource = "Gemini";
                        }
                        else
                        {
                            result.RerankScore = 0; // Penalize if AI dropped it entirely
                            result.FinalScore = 0;
                        }
                    }

                    // Re-sort by AI score
                    results = results.OrderByDescending(r => r.FinalScore).ToList();
                }
                else
                {
                    // Fallback to Pinecone sorting
                    results = results.OrderByDescending(r => r.FinalScore).ToList();
                }
            }

            return results;
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
