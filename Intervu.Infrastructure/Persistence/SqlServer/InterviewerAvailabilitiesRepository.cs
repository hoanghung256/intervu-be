using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class InterviewerAvailabilitiesRepository : IInterviewerAvailabilitiesRepository
    {
        private readonly IntervuDbContext _dbContext;
        public InterviewerAvailabilitiesRepository(IntervuDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<InterviewerAvailability>> GetInterviewerAvailabilitiesByMonthAsync(int intervewerId, int month = 0, int year = 0)
        {
            var query = _dbContext.InterviewerAvailabilities.AsQueryable();

            var result = await query.Where(x => x.InterviewerId == intervewerId)
                .Where(x => x.StartTime.Month == month)
                .Where(x => x.StartTime.Year == year)
                .ToListAsync();
            return result;
        }

        public Task<bool> IsInterviewerAvailableAsync(int interviewerId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            throw new NotImplementedException();
        }
    }
}
