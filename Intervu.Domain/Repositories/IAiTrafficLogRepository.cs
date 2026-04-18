using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IAiTrafficLogRepository : IRepositoryBase<AiTrafficLog>
    {
        Task<IEnumerable<AiTrafficLog>> GetByTimeframeAsync(DateTime from, DateTime to);

        Task<(IReadOnlyList<AiTrafficLog> Items, int TotalCount)> QueryAsync(
            DateTime from,
            DateTime to,
            string? provider,
            string? endpointContains,
            string? useCase,
            int page,
            int pageSize);

        Task<IReadOnlyList<string>> GetDistinctProvidersAsync();

        Task<IReadOnlyList<string>> GetDistinctEndpointsAsync();

        Task<IReadOnlyList<string>> GetDistinctUseCasesAsync();
    }
}
