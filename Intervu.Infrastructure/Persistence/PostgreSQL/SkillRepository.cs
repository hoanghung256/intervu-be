using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class SkillRepository(IntervuPostgreDbContext context) : ISkillRepository
    {
        private readonly IntervuPostgreDbContext _context = context;
        
        public async Task<(IReadOnlyList<Skill> Items, int TotalCount)> GetPagedSkillsAsync(int page, int pageSize)
        {
            var query = _context.Skills.AsQueryable();

            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<IReadOnlyList<Skill>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids == null) return new List<Skill>();
            var list = await _context.Skills.Where(s => ids.Contains(s.Id)).ToListAsync();
            return list;
        }
    }
}
