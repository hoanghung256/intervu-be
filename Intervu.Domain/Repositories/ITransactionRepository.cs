using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ITransactionRepository : IRepositoryBase<InterviewBookingTransaction>
    {
        Task<InterviewBookingTransaction?> GetByAvailabilityId(int id);
    }
}
