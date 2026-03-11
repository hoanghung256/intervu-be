using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ITagRepository : IRepositoryBase<Tag>
    {
        Task<List<Tag>> GetAllAsync();
        Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<Tag?> GetByNameAsync(string name);
    }
}
