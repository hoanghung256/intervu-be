using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class IndustryRepository(IntervuPostgreDbContext context) : IIndustryRepository
    {
        private readonly IntervuPostgreDbContext _context = context;

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
