using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        void DeleteAsync(T entity);
        Task<int> SaveChangesAsync();
        void SoftDeleteAsync(T entity);
    }
}
