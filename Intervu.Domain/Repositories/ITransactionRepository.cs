using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface ITransactionRepository : IRepositoryBase<InterviewBookingTransaction>
    {
        Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id);

        Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id, TransactionType type);

        Task<InterviewBookingTransaction?> Get(int orderCode, TransactionType type);
    }
}
