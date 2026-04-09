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

        public async Task<InterviewBookingTransaction?> GetByBookingRequestId(Guid bookingRequestId, TransactionType type)
        {
            return await _context.InterviewBookingTransaction
                .FirstOrDefaultAsync(t => t.BookingRequestId == bookingRequestId && t.Type == type);
        }

        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid availabilityId, TransactionType type)
        {
            return await (
                from transaction in _context.InterviewBookingTransaction
                join round in _context.InterviewRounds
                    on transaction.BookingRequestId equals (Guid?)round.BookingRequestId
                join availability in _context.CoachAvailabilities
                    on round.Id equals availability.InterviewRoundId
                where availability.Id == availabilityId && transaction.Type == type
                select transaction
            ).FirstOrDefaultAsync();
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
                .Include(t => t.BookingRequest)
                    .ThenInclude(br => br!.Rounds.OrderBy(r => r.RoundNumber))
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

        public async Task<int> GetTotalPayoutByUserAsync(Guid userId, DateTime from, DateTime to)
        {
            return await _context.InterviewBookingTransaction
                .Where(t => t.UserId == userId
                    && t.Type == TransactionType.Payout
                    && t.Status == TransactionStatus.Paid
                    && t.OrderCode >= 0) // valid transactions
                .Join(_context.BookingRequests,
                    t => t.BookingRequestId,
                    br => br.Id,
                    (t, br) => new { t, br })
                .Where(x => x.br.RespondedAt >= from && x.br.RespondedAt < to)
                .SumAsync(x => x.t.Amount);
        }

        public async Task<List<(DateTime Date, int Amount)>> GetDailyPayoutByUserAsync(Guid userId, DateTime from, DateTime to)
        {
            var results = await _context.InterviewBookingTransaction
                .Where(t => t.UserId == userId
                    && t.Type == TransactionType.Payout
                    && t.Status == TransactionStatus.Paid)
                .Join(_context.BookingRequests,
                    t => t.BookingRequestId,
                    br => br.Id,
                    (t, br) => new { t, br })
                .Where(x => x.br.RespondedAt >= from && x.br.RespondedAt < to)
                .GroupBy(x => x.br.RespondedAt!.Value.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.t.Amount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return results.Select(r => (r.Date, r.Amount)).ToList();
        }
    }
}
