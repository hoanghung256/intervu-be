using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Intervu.Application.UseCases.Admin
{
    public class GetPineconeIndexStats : IGetPineconeIndexStats
    {
        private readonly IVectorStoreService _vectorStoreService;
        private readonly IUserRepository _userRepository;
        private readonly IQuestionRepository _questionRepository;

        public GetPineconeIndexStats(
            IVectorStoreService vectorStoreService,
            IUserRepository userRepository,
            IQuestionRepository questionRepository)
        {
            _vectorStoreService = vectorStoreService;
            _userRepository = userRepository;
            _questionRepository = questionRepository;
        }

        public async Task<PineconeIndexStatsDto> ExecuteAsync()
        {
            var stats = await _vectorStoreService.DescribeIndexStatsAsync();

            var coachCount = await _userRepository.GetActiveCoachCountAsync();
            var questionCount = await _questionRepository.GetApprovedCountAsync();

            // Inject SQL Counts into the namespaces dictionary
            if (stats.Namespaces.TryGetValue("coaches", out var coachesNs))
            {
                coachesNs.SqlCount = coachCount;
            }
            else
            {
                stats.Namespaces["coaches"] = new NamespaceComparisonDto { SqlCount = coachCount };
            }

            if (stats.Namespaces.TryGetValue("questions", out var questionsNs))
            {
                questionsNs.SqlCount = questionCount;
            }
            else
            {
                stats.Namespaces["questions"] = new NamespaceComparisonDto { SqlCount = questionCount };
            }

            return stats;
        }
    }
}
