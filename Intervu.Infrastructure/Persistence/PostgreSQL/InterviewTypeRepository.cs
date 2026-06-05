using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewTypeRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewType>(context), IInterviewTypeRepository
    {
        public async Task<InterviewType?> GetByNameAsync(string name)
        {
            var normalized = name.Trim().ToLower();
            return await _context.InterviewTypes
                .FirstOrDefaultAsync(it => it.Name.ToLower() == normalized);
        }

        public async Task<(IEnumerable<InterviewType> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool activeOnly)
        {
            IQueryable<InterviewType> query = _context.InterviewTypes.AsNoTracking();
            if (activeOnly)
                query = query.Where(it => it.Status == InterviewTypeStatus.Active);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(it => it.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
