using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.UseCases.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetPineconeIndexStats : IGetPineconeIndexStats
    {
        private readonly IVectorStoreService _vectorStoreService;

        public GetPineconeIndexStats(IVectorStoreService vectorStoreService)
        {
            _vectorStoreService = vectorStoreService;
        }

        public Task<PineconeIndexStatsDto> ExecuteAsync() =>
            _vectorStoreService.DescribeIndexStatsAsync();
    }
}
