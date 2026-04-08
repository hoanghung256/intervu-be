using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class CoachInterviewServiceRepository(IntervuPostgreDbContext context)
        : RepositoryBase<CoachInterviewService>(context), ICoachInterviewServiceRepository
    {
        public async Task<IEnumerable<CoachInterviewService>> GetByCoachIdAsync(Guid coachId)
        {
            return await _context.CoachInterviewServices
                .Include(s => s.InterviewType)
                .Where(s => s.CoachId == coachId)
                .ToListAsync();
        }

        public async Task<CoachInterviewService?> GetByCoachAndTypeAsync(Guid coachId, Guid interviewTypeId)
        {
            return await _context.CoachInterviewServices
                .Include(s => s.InterviewType)
                .FirstOrDefaultAsync(s => s.CoachId == coachId && s.InterviewTypeId == interviewTypeId);
        }

        public async Task<CoachInterviewService?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.CoachInterviewServices
                .Include(s => s.InterviewType)
                .Include(s => s.CoachProfile)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<CoachInterviewService>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.CoachInterviewServices
                .Include(s => s.InterviewType)
                .Where(s => ids.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<bool> HasActiveReferencesAsync(Guid serviceId)
        {
            return await _context.BookingRequests
                       .AnyAsync(b => b.CoachInterviewServiceId == serviceId)
                || await _context.InterviewRounds
                       .AnyAsync(r => r.CoachInterviewServiceId == serviceId)
                || await _context.InterviewRooms
                       .AnyAsync(r => r.CoachInterviewServiceId == serviceId);
        }
    }
}
