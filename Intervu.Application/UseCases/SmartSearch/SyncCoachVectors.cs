using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Repositories;
using CoachProfileEntity = Intervu.Domain.Entities.CoachProfile;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SyncCoachVectors : ISyncCoachVectors
    {
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStoreService _vectorStoreService;

        public SyncCoachVectors(
            ICoachProfileRepository coachProfileRepository,
            IEmbeddingService embeddingService,
            IVectorStoreService vectorStoreService)
        {
            _coachProfileRepository = coachProfileRepository;
            _embeddingService = embeddingService;
            _vectorStoreService = vectorStoreService;
        }

        public async Task<int> ExecuteAsync()
        {
            // Fetch all coaches with related data
            var (coaches, totalCount) = await _coachProfileRepository.GetPagedCoachProfilesAsync(
                search: null, skillId: null, companyId: null, page: 1, pageSize: 1000);

            int syncedCount = 0;

            foreach (var coach in coaches)
            {
                var documentText = BuildCoachDocumentText(coach);
                var vector = await _embeddingService.GetEmbeddingAsync(documentText);
                var metadata = BuildMetadata(coach);

                await _vectorStoreService.UpsertAsync(coach.Id.ToString(), vector, metadata);
                syncedCount++;
            }

            return syncedCount;
        }

        /// <summary>
        /// Builds a rich text document from coach profile data for AI embedding.
        /// This is the core of the semantic search — the richer the text, the better the search quality.
        /// </summary>
        private static string BuildCoachDocumentText(CoachProfileEntity coach)
        {
            var parts = new List<string>();

            // Coach name
            if (coach.User != null)
                parts.Add($"Coach: {coach.User.FullName}");

            // Bio
            if (!string.IsNullOrWhiteSpace(coach.Bio))
                parts.Add($"Bio: {coach.Bio}");

            // Experience
            if (coach.ExperienceYears.HasValue)
                parts.Add($"Experience: {coach.ExperienceYears} years");

            // Skills
            if (coach.Skills.Any())
                parts.Add($"Skills: {string.Join(", ", coach.Skills.Select(s => s.Name))}");

            // Companies
            if (coach.Companies.Any())
                parts.Add($"Companies: {string.Join(", ", coach.Companies.Select(c => c.Name))}");

            // Interview services offered
            if (coach.InterviewServices.Any())
            {
                var services = coach.InterviewServices
                    .Where(s => s.InterviewType != null)
                    .Select(s => s.InterviewType.Name);
                if (services.Any())
                    parts.Add($"Interview types: {string.Join(", ", services)}");
            }

            return string.Join(". ", parts);
        }

        private static Dictionary<string, string> BuildMetadata(CoachProfileEntity coach)
        {
            var metadata = new Dictionary<string, string>
            {
                { "coachId", coach.Id.ToString() },
                { "name", coach.User?.FullName ?? "" },
                { "bio", coach.Bio ?? "" }
            };

            if (coach.Skills.Any())
                metadata["skills"] = string.Join(", ", coach.Skills.Select(s => s.Name));

            if (coach.Companies.Any())
                metadata["companies"] = string.Join(", ", coach.Companies.Select(c => c.Name));

            if (coach.ExperienceYears.HasValue)
                metadata["experienceYears"] = coach.ExperienceYears.Value.ToString();

            return metadata;
        }
    }
}
