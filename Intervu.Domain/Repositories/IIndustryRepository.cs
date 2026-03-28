using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IIndustryRepository
    {
        Task<IReadOnlyList<Industry>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
