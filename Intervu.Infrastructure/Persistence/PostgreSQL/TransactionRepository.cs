using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class TransactionRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewBookingTransaction>(context), ITransactionRepository
    {
        // Include a safety window on month boundaries so slots near midnight
        // are not dropped when clients render in non-UTC time zones.
        private const int MonthBoundaryTimezoneBufferHours = 14;

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

        public async Task<(IReadOnlyList<InterviewBookingTransaction> Items, int TotalItems)> GetListByUserAsync(
            Guid userId,
            int page,
            int pageSize,
            TransactionType? type = null,
            TransactionStatus? status = null)
        {
            var query = _context.InterviewBookingTransaction
                .AsNoTracking()
                .Include(t => t.CoachAvailability)
                .Include(t => t.BookingRequest)
                    .ThenInclude(br => br.Rounds)
                .Where(t => t.UserId == userId);

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.OrderCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
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

        //public async Task<List<InterviewBookingTransaction>> GetActiveBookingsByCoachAsync(
        //    Guid coachId, DateTime rangeStart, DateTime rangeEnd)
        //{
        //    return await _context.InterviewBookingTransaction
        //        .Where(t =>
        //            t.CoachId == coachId
        //            && t.Type == TransactionType.Payment
        //            && (t.Status == TransactionStatus.Created || t.Status == TransactionStatus.Paid)
        //            && t.BookedStartTime.HasValue
        //            && t.BookedDurationMinutes.HasValue
        //            && t.BookedStartTime.Value < rangeEnd
        //            && t.BookedStartTime.Value.AddMinutes(t.BookedDurationMinutes.Value) > rangeStart
        //        )
        //        .AsNoTracking()
        //        .ToListAsync();
        //}

        public Task<List<InterviewBookingTransaction>> GetActiveBookingsByCoachAsync(Guid coachId, DateTime rangeStart, DateTime rangeEnd)
        {
            return _context.InterviewBookingTransaction
                .Where(t => t.CoachId == coachId)
                .Where(t => t.Type == TransactionType.Payment)
                .Where(t => t.Status == TransactionStatus.Created || t.Status == TransactionStatus.Paid)
                .Where(t => t.BookedStartTime.HasValue && t.BookedDurationMinutes.HasValue)
                .Where(t => t.BookedStartTime.Value < rangeEnd && t.BookedStartTime.Value.AddMinutes(t.BookedDurationMinutes.Value) > rangeStart)
                .ToListAsync();
        }

        public async Task<List<(DateTime Start, DateTime End)>> GetConfirmedBookingsForCoachAsync(Guid coachId, int month, int year)
        {
            var query = _context.InterviewBookingTransaction
                .Where(t => t.CoachId == coachId)
                .Where(t => t.Type == TransactionType.Payment)
                .Where(t => t.Status == TransactionStatus.Created || t.Status == TransactionStatus.Paid)
                .Where(t => t.BookedStartTime.HasValue && t.BookedDurationMinutes.HasValue);

            if (month > 0 && year > 0)
            {
                var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEndUtc = monthStartUtc.AddMonths(1);
                var queryStartUtc = monthStartUtc.AddHours(-MonthBoundaryTimezoneBufferHours);
                var queryEndUtc = monthEndUtc.AddHours(MonthBoundaryTimezoneBufferHours);

                query = query.Where(t => t.BookedStartTime >= queryStartUtc && t.BookedStartTime < queryEndUtc);
            }

            return await query
                .Select(t => new ValueTuple<DateTime, DateTime>(t.BookedStartTime!.Value, t.BookedStartTime!.Value.AddMinutes(t.BookedDurationMinutes!.Value)))
                .ToListAsync();
        }

        public async Task<List<InterviewBookingTransaction>> GetConfirmedBookingEntitiesForCoachAsync(Guid coachId, int month, int year)
        {
            var query = _context.InterviewBookingTransaction
                .Include(t => t.BookingRequest)
                    .ThenInclude(br => br!.Candidate)
                        .ThenInclude(c => c.User)
                .Where(t => t.CoachId == coachId)
                .Where(t => t.Type == TransactionType.Payment)
                .Where(t => t.Status == TransactionStatus.Created || t.Status == TransactionStatus.Paid)
                .Where(t => t.BookedStartTime.HasValue && t.BookedDurationMinutes.HasValue);

            if (month > 0 && year > 0)
            {
                var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEndUtc = monthStartUtc.AddMonths(1);
                var queryStartUtc = monthStartUtc.AddHours(-MonthBoundaryTimezoneBufferHours);
                var queryEndUtc = monthEndUtc.AddHours(MonthBoundaryTimezoneBufferHours);

                query = query.Where(t => t.BookedStartTime >= queryStartUtc && t.BookedStartTime < queryEndUtc);
            }

            return await query.ToListAsync();
        }
    }
}
