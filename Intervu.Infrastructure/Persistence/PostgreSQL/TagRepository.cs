using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class TagRepository : RepositoryBase<Tag>, ITagRepository
    {
        public TagRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<List<Tag>> GetAllAsync()
        {
            return await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Tags.Where(t => ids.Contains(t.Id)).ToListAsync();
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }
    }
}
