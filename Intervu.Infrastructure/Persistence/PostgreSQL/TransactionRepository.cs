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
    }
}
