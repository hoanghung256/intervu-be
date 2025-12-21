using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public abstract class RepositoryBase<T>(IntervuPostgreDbContext context) : IRepositoryBase<T> where T : class
    {
        protected readonly IntervuPostgreDbContext _context = context;

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        public void DeleteAsync(T entity) => _context.Set<T>().Remove(entity);

        public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);

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
