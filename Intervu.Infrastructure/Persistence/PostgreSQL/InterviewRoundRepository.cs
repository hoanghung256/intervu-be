using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class InterviewRoundRepository(IntervuPostgreDbContext context)
        : RepositoryBase<InterviewRound>(context), IInterviewRoundRepository
    {
        public async Task<IEnumerable<InterviewRound>> GetByBookingRequestIdAsync(Guid bookingRequestId)
        {
            return await _context.InterviewRounds
                .Include(r => r.CoachInterviewService)
                    .ThenInclude(s => s.InterviewType)
                .Where(r => r.BookingRequestId == bookingRequestId)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();
        }
    }
}
