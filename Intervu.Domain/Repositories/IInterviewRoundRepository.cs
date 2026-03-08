using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IInterviewRoundRepository : IRepositoryBase<InterviewRound>
    {
        Task<IEnumerable<InterviewRound>> GetByBookingRequestIdAsync(Guid bookingRequestId);
    }
}
