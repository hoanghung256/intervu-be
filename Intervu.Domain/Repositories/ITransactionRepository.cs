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
    }
}
