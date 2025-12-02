using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly IntervuDbContext _context;

        protected RepositoryBase(IntervuDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        public void DeleteAsync(T entity) => _context.Set<T>().Remove(entity);

        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);

        public void SoftDeleteAsync(T entity)
        {
            if (entity is ISoftDelete deletableEntity)
            {
                deletableEntity.IsDeleted = true;
            }
            else
            {
                throw new InvalidOperationException("Entity does not support soft delete.");
            }
        }

        public void UpdateAsync(T entity) => _context.Set<T>().Update(entity);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
