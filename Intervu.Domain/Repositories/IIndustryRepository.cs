using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IIndustryRepository
    {
        Task<(IReadOnlyList<Industry> Items, int TotalCount)> GetPagedIndustriesAsync(int page, int pageSize);
        Task<IReadOnlyList<Industry>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
