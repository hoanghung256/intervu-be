using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class CoachAvailabilitiesRepository(IntervuPostgreDbContext context) : RepositoryBase<CoachAvailability>(context), ICoachAvailabilitiesRepository
    {
        public async Task<IEnumerable<CoachAvailability>> GetCoachAvailabilitiesByMonthAsync(
            Guid coachId, 
            int month = 0, 
            int year = 0)
        {
            var query = _context.CoachAvailabilities.AsQueryable();
            var filtered = query.Where(x => x.CoachId == coachId);

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

        public async Task<bool> UpdateCoachAvailabilityAsync(Guid availabilityId, InterviewFocus focus,DateTimeOffset startTime, DateTimeOffset endTime, Guid? typeId)
        {
            var availability = await _context.CoachAvailabilities.FindAsync(availabilityId);
            if (availability == null)
                return false;

            availability.Focus = focus;
            availability.StartTime = startTime.UtcDateTime;
            availability.EndTime = endTime.UtcDateTime;
            availability.TypeId = typeId;

            _context.CoachAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime)
        {
            return _context.CoachAvailabilities
                .FirstOrDefaultAsync(a => a.CoachId == coachId && a.StartTime == startTime);
        }

        public async Task ExpireReservedSlot(Guid availabilityId, Guid reseverForUserId)
        {
            var slot = await _context.CoachAvailabilities.FindAsync(availabilityId);
            if (slot == null || slot.Status != CoachAvailabilityStatus.Reserved || slot.ReservingForUserId != reseverForUserId)
            {
                return;
            }

            slot.Status = CoachAvailabilityStatus.Available;
            slot.ReservingForUserId = null;
            await _context.SaveChangesAsync();
        }

        public async Task ReserveForSlot(Guid availabilityId, Guid reseverForUserId)
        {
            var slot = await _context.CoachAvailabilities.FindAsync(availabilityId);
            if (slot == null || slot.Status != CoachAvailabilityStatus.Reserved || slot.ReservingForUserId != reseverForUserId)
            {
                return;
            }

            slot.Status = CoachAvailabilityStatus.Reserved;
            slot.ReservingForUserId = reseverForUserId;
            await _context.SaveChangesAsync();
        }
    }
}
