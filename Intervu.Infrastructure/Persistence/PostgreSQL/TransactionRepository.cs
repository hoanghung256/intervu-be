using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class TransactionRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewBookingTransaction>(context), ITransactionRepository
    {
        public async Task<InterviewBookingTransaction?> Get(int orderCode, TransactionType type)
        {
            return await _context.InterviewBookingTransaction.FirstOrDefaultAsync(t => t.OrderCode == orderCode && t.Type == type);
        }

        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id)
        {
            return await _context.InterviewBookingTransaction.FirstOrDefaultAsync(t => t.CoachAvailabilityId == id);
        }

        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id, TransactionType type)
        {
            return await _context.InterviewBookingTransaction
                .FirstOrDefaultAsync(t => t.CoachAvailabilityId == id && t.Type == type);
        }

        public async Task<bool> HasOverlappingBookingAsync(Guid coachId, DateTime startTime, DateTime endTime)
        {
            return await _context.InterviewBookingTransaction
                .AnyAsync(t =>
                    t.CoachId == coachId
                    && t.Type == TransactionType.Payment
                    && (t.Status == TransactionStatus.Created || t.Status == TransactionStatus.Paid)
                    && t.BookedStartTime.HasValue
                    && t.BookedDurationMinutes.HasValue
                    && t.BookedStartTime.Value < endTime
                    && t.BookedStartTime.Value.AddMinutes(t.BookedDurationMinutes.Value) > startTime
                );
        }

        public async Task<List<InterviewBookingTransaction>> GetActiveBookingsByCoachAsync(
            Guid coachId, DateTime rangeStart, DateTime rangeEnd)
        {
            return await _context.InterviewBookingTransaction
                .Where(t =>
                    t.CoachId == coachId
                    && t.Type == TransactionType.Payment
                    && (t.Status == TransactionStatus.Created || t.Status == TransactionStatus.Paid)
                    && t.BookedStartTime.HasValue
                    && t.BookedDurationMinutes.HasValue
                    && t.BookedStartTime.Value < rangeEnd
                    && t.BookedStartTime.Value.AddMinutes(t.BookedDurationMinutes.Value) > rangeStart
                )
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
