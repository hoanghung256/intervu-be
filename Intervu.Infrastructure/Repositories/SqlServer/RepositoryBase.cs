using Intervu.Domain.Abstractions.Repository.Interfaces;
using Intervu.Infrastructure.Repositories.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Repositories.SqlServer
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly IntervuDbContext _context;

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        public void DeleteAsync(T entity) => _context.Set<T>().Remove(entity);

        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);

        public void UpdateAsync(T entity) => _context.Set<T>().Update(entity);
    }
}
