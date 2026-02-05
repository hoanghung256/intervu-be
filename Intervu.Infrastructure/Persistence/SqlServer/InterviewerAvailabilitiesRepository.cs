using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class InterviewerAvailabilitiesRepository : RepositoryBase<CoachAvailability>, ICoachAvailabilitiesRepository
    {
        private readonly IntervuDbContext _dbContext;
        public InterviewerAvailabilitiesRepository(IntervuDbContext dbContext) :base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CoachAvailability>> GetCoachAvailabilitiesByMonthAsync(Guid coachId, int month = 0, int year = 0)
        {
            var query = _dbContext.CoachAvailabilities.AsQueryable();

            var filtered = query.Where(x => x.CoachId == coachId && x.Status == CoachAvailabilityStatus.Available);

            if (month > 0 && year > 0)
            {
                filtered = filtered.Where(x => x.StartTime.Month == month && x.StartTime.Year == year);
            }
            else if (month > 0)
            {
                filtered = filtered.Where(x => x.StartTime.Month == month);
            }
            else if (year > 0)
            {
                filtered = filtered.Where(x => x.StartTime.Year == year);
            }

            var result = await filtered.ToListAsync();
            return result;
        }

        public async Task<bool> IsCoachAvailableAsync(Guid coachId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            // return true if no overlapping availability or booking exists
            var overlaps = await _dbContext.CoachAvailabilities
                .Where(x => x.CoachId == coachId)
                .Where(x => !(x.EndTime <= startTime.UtcDateTime || x.StartTime >= endTime.UtcDateTime))
                .AnyAsync();
            return !overlaps;
        }

        public async Task<Guid> CreateCoachAvailabilityAsync(CoachAvailability availability)
        {
            _dbContext.CoachAvailabilities.Add(availability);
            await _dbContext.SaveChangesAsync();
            return availability.Id;
        }

        public async Task<Guid> CreateMultipleCoachAvailabilitiesAsync(List<CoachAvailability> availabilities)
        {
            _dbContext.CoachAvailabilities.AddRange(availabilities);
            await _dbContext.SaveChangesAsync();
            return availabilities.First().Id;
        }

        public async Task<bool> DeleteCoachAvailabilityAsync(Guid availabilityId)
        {
            var availability = await _dbContext.CoachAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            _dbContext.CoachAvailabilities.Remove(availability);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCoachAvailabilityAsync(Guid availabilityId, DateTimeOffset startTime, DateTimeOffset endTime, Guid typeId)
        {
            var availability = await _context.CoachAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            availability.StartTime = startTime.UtcDateTime;
            availability.EndTime = endTime.UtcDateTime;
            availability.TypeId = typeId;

            _context.CoachAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime)
        {
            return _dbContext.CoachAvailabilities
                .FirstOrDefaultAsync(a => a.CoachId == coachId && a.StartTime == startTime);
        }
    }
}
