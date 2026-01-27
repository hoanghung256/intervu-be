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

        public async Task<IEnumerable<InterviewType>> GetList(int page, int pageSize)
        {
            return await _context.InterviewTypes
                // .Where(it => it.Status == InterviewTypeStatus.Active)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(it => it.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
