using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class CoachAvailabilitiesRepository(IntervuPostgreDbContext context) : RepositoryBase<CoachAvailability>(context), ICoachAvailabilitiesRepository
    {
        // Include a safety window on month boundaries so slots near midnight
        // are not dropped when clients render in non-UTC time zones.
        private const int MonthBoundaryTimezoneBufferHours = 14;

        public async Task<IEnumerable<CoachAvailability>> GetCoachAvailabilitiesByMonthAsync(
            Guid coachId, 
            int month = 0, 
            int year = 0)
        {
            var query = _context.CoachAvailabilities.AsQueryable();
            var filtered = query.Where(x => x.CoachId == coachId && x.Status == CoachAvailabilityStatus.Available);

            if (month > 0 && year > 0)
            {
                var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEndUtc = monthStartUtc.AddMonths(1);
                var queryStartUtc = monthStartUtc.AddHours(-MonthBoundaryTimezoneBufferHours);
                var queryEndUtc = monthEndUtc.AddHours(MonthBoundaryTimezoneBufferHours);

                filtered = filtered.Where(x => x.StartTime >= queryStartUtc && x.StartTime < queryEndUtc);
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

        public async Task<bool> IsCoachAvailableAsync(Guid coachId, DateTimeOffset startTime, DateTimeOffset endTime, Guid? excludeId = null)
        {
            // return true if no overlapping availability or booking exists
            var overlaps = await _context.CoachAvailabilities
                .Where(x => x.CoachId == coachId)
                .Where(x => !(x.EndTime <= startTime.UtcDateTime || x.StartTime >= endTime.UtcDateTime))
                .Where(x => x.Id != excludeId)
                .AnyAsync();
            return !overlaps;
        }

        public async Task<Guid> CreateCoachAvailabilityAsync(CoachAvailability availability)
        {
            _context.CoachAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            return availability.Id;
        }

        public async Task<Guid> CreateMultipleCoachAvailabilitiesAsync(List<CoachAvailability> availabilities)
        {
            _context.CoachAvailabilities.AddRange(availabilities);
            await _context.SaveChangesAsync();
            return availabilities.First().Id;
        }

        public async Task<bool> DeleteCoachAvailabilityAsync(Guid availabilityId)
        {
            var availability = await _context.CoachAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            _context.CoachAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCoachAvailabilityAsync(Guid availabilityId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var availability = await _context.CoachAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            availability.StartTime = startTime.UtcDateTime;
            availability.EndTime = endTime.UtcDateTime;

            _context.CoachAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime)
        {
            return _context.CoachAvailabilities
                .FirstOrDefaultAsync(a => a.CoachId == coachId && a.StartTime == startTime);
        }

        public Task<CoachAvailability?> GetByIdForUpdateAsync(Guid availabilityId)
        {
            if (_context.Database.IsInMemory())
            {
                return _context.CoachAvailabilities.FirstOrDefaultAsync(x => x.Id == availabilityId);
            }

            return _context.CoachAvailabilities
                .FromSqlInterpolated($@"SELECT * FROM ""CoachAvailabilities"" WHERE ""Id"" = {availabilityId} FOR UPDATE")
                .FirstOrDefaultAsync();
        }

        public async Task<List<CoachAvailability>> GetBlocksInRangeAsync(Guid coachId, DateTime startTime, DateTime endTime)
        {
            return await _context.CoachAvailabilities
                .Where(x => x.CoachId == coachId
                    && x.StartTime >= startTime
                    && x.EndTime <= endTime)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public Task<List<CoachAvailability>> GetBlocksInRangeForUpdateAsync(Guid coachId, DateTime startTime, DateTime endTime)
        {
            if (_context.Database.IsInMemory())
            {
                return _context.CoachAvailabilities
                    .Where(x => x.CoachId == coachId
                        && x.StartTime >= startTime
                        && x.EndTime <= endTime)
                    .OrderBy(x => x.StartTime)
                    .ToListAsync();
            }

            return _context.CoachAvailabilities
                .FromSqlInterpolated($@"SELECT * FROM ""CoachAvailabilities""
                    WHERE ""CoachId"" = {coachId}
                      AND ""StartTime"" >= {startTime}
                      AND ""EndTime"" <= {endTime}
                    ORDER BY ""StartTime""
                    FOR UPDATE")
                .ToListAsync();
        }

        public async Task<int> DeleteMultipleAsync(List<Guid> ids)
        {
            var blocks = await _context.CoachAvailabilities
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            _context.CoachAvailabilities.RemoveRange(blocks);
            await _context.SaveChangesAsync();
            return blocks.Count;
        }
    }
}
