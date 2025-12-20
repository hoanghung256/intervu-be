using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewerAvailabilitiesRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewerAvailability>(context), IInterviewerAvailabilitiesRepository
    {
        public async Task<IEnumerable<InterviewerAvailability>> GetInterviewerAvailabilitiesByMonthAsync(
            int intervewerId, 
            int month = 0, 
            int year = 0)
        {
            var query = _context.InterviewerAvailabilities.AsQueryable();

            var filtered = query.Where(x => x.InterviewerId == intervewerId && x.IsBooked == false);

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

        public async Task<bool> IsInterviewerAvailableAsync(int interviewerId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            // return true if no overlapping availability or booking exists
            var overlaps = await _context.InterviewerAvailabilities
                .Where(x => x.InterviewerId == interviewerId)
                .Where(x => !(x.EndTime <= startTime.UtcDateTime || x.StartTime >= endTime.UtcDateTime))
                .AnyAsync();
            return !overlaps;
        }

        public async Task<int> CreateInterviewerAvailabilityAsync(InterviewerAvailability availability)
        {
            _context.InterviewerAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            return availability.Id;
        }

        public async Task<int> CreateMultipleInterviewerAvailabilitiesAsync(List<InterviewerAvailability> availabilities)
        {
            _context.InterviewerAvailabilities.AddRange(availabilities);
            await _context.SaveChangesAsync();
            return availabilities.First().Id;
        }

        public async Task<bool> DeleteInterviewerAvailabilityAsync(int availabilityId)
        {
            var availability = await _context.InterviewerAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            _context.InterviewerAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateInterviewerAvailabilityAsync(int availabilityId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var availability = await _context.InterviewerAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            availability.StartTime = startTime.UtcDateTime;
            availability.EndTime = endTime.UtcDateTime;

            _context.InterviewerAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<InterviewerAvailability?> GetAsync(int interviewerId, DateTime startTime)
        {
            return _context.InterviewerAvailabilities
                .FirstOrDefaultAsync(a => a.InterviewerId == interviewerId && a.StartTime == startTime);
        }
    }
}
