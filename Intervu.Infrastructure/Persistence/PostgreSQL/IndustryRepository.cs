using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class IndustryRepository(IntervuPostgreDbContext context) : IIndustryRepository
    {
        private readonly IntervuPostgreDbContext _context = context;

        public async Task<(IReadOnlyList<Industry> Items, int TotalCount)> GetPagedIndustriesAsync(int page, int pageSize)
        {
            var query = _context.Industries.AsQueryable();
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IReadOnlyList<Industry>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return new List<Industry>();
            }

            return await _context.Industries
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }
    }
}
