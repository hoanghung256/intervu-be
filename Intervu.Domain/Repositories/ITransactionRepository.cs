using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface ITransactionRepository : IRepositoryBase<InterviewBookingTransaction>
    {
        Task<InterviewBookingTransaction?> Get(int orderCode, TransactionType type);

        Task<InterviewBookingTransaction?> GetByBookingRequestId(Guid bookingRequestId, TransactionType type);

        Task<(IReadOnlyList<InterviewBookingTransaction> Items, int TotalItems)> GetListByUserAsync(
            Guid userId,
            int page,
            int pageSize,
            TransactionType? type = null,
            TransactionStatus? status = null);

        Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid availabilityId, TransactionType type);

        /// <summary>
        /// Sum payout amounts for a coach within a date range.
        /// </summary>
        Task<int> GetTotalPayoutByUserAsync(Guid userId, DateTime from, DateTime to);

        /// <summary>
        /// Daily payout breakdown for a coach within a date range.
        /// Returns (Date, TotalAmount) pairs.
        /// </summary>
        Task<List<(DateTime Date, int Amount)>> GetDailyPayoutByUserAsync(Guid userId, DateTime from, DateTime to);
    }
}
