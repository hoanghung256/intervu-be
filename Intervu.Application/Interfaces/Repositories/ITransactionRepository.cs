using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IRepositoryBase<InterviewBookingTransaction>
    {
        Task<InterviewBookingTransaction?> GetByAvailabilityId(int id);
    }
}
