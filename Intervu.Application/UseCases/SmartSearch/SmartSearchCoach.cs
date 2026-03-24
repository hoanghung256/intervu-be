using AutoMapper;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SmartSearchCoach : ISmartSearchCoach
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStoreService _vectorStoreService;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IMapper _mapper;

        public SmartSearchCoach(
            IEmbeddingService embeddingService,
            IVectorStoreService vectorStoreService,
            ICoachProfileRepository coachProfileRepository,
            IMapper mapper)
        {
            _embeddingService = embeddingService;
            _vectorStoreService = vectorStoreService;
            _coachProfileRepository = coachProfileRepository;
            _mapper = mapper;
        }

        public async Task<List<SmartSearchResultDto>> ExecuteAsync(SmartSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                throw new ArgumentException("Search query cannot be empty.");

            // Use query embedding mode for user text.
            var queryVector = await _embeddingService.GetEmbeddingAsync(request.Query, "query");

            // Search only in coach vectors.
            var vectorMatches = await _vectorStoreService.SearchAsync(
                queryVector,
                request.TopK,
                request.Namespace,
                new Dictionary<string, string> { ["entityType"] = "coach" });

            if (!vectorMatches.Any())
                return new List<SmartSearchResultDto>();

            var results = new List<SmartSearchResultDto>();

            foreach (var match in vectorMatches)
            {
                if (!IsValidCoachMatch(match))
                {
                    continue;
                }

                if (Guid.TryParse(match.Id, out var coachId))
                {
                    var coachProfile = await _coachProfileRepository.GetProfileByIdAsync(coachId);
                    if (coachProfile != null)
                    {
                        var coachDto = _mapper.Map<CoachViewDto>(coachProfile);
                        results.Add(new SmartSearchResultDto
                        {
                            CoachId = coachId,
                            MatchScore = match.Score,
                            Coach = coachDto
                        });
                    }
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
