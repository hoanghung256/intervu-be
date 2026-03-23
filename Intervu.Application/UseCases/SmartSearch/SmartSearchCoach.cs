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

            // Step 1: Embed the query text
            var queryVector = await _embeddingService.GetEmbeddingAsync(request.Query);

            // Step 2: Search Pinecone for similar vectors
            var vectorMatches = await _vectorStoreService.SearchAsync(queryVector, request.TopK);

            if (!vectorMatches.Any())
                return new List<SmartSearchResultDto>();

            // Step 3: Fetch coach details from PostgreSQL
            var results = new List<SmartSearchResultDto>();

            foreach (var match in vectorMatches)
            {
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
    }
}
