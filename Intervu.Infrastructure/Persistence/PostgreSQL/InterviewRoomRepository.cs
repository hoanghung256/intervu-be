using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewRoomRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewRoom>(context), IInterviewRoomRepository
    {
        public async Task<IEnumerable<InterviewRoom>> GetListByCandidateId(Guid candidateId)
        {
            return await _context.InterviewRooms.Where(r => r.CandidateId == candidateId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetListByCoachId(Guid coachId)
        {
            return await _context.InterviewRooms.Where(r => r.CoachId == coachId).ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetList()
        {
            return await _context.InterviewRooms.ToListAsync();
        }

        public async Task<IEnumerable<InterviewRoom>> GetConflictingRoomsAsync(Guid userId, DateTime startTime, DateTime endTime)
        {
            return await _context.InterviewRooms
                .Where(r => 
                    (r.CandidateId == userId || r.CoachId == userId) &&
                    r.ScheduledTime != null &&
                    r.Status != InterviewRoomStatus.Cancelled &&
                    r.Status != InterviewRoomStatus.Completed &&
                    // Check overlap: (StartA < EndB) and (EndA > StartB)
                    r.ScheduledTime < endTime &&
                    r.ScheduledTime.Value.AddMinutes(r.DurationMinutes ?? 60) > startTime)
                .ToListAsync();
        }
    }
}
