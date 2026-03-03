using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface IBookingRequestRepository : IRepositoryBase<BookingRequest>
    {
        Task<BookingRequest?> GetByIdWithDetailsAsync(Guid id);
        Task<(IReadOnlyList<BookingRequest> Items, int TotalCount)> GetPagedByCandidateIdAsync(
            Guid candidateId, BookingRequestType? type, List<BookingRequestStatus>? statuses, int page, int pageSize);
        Task<(IReadOnlyList<BookingRequest> Items, int TotalCount)> GetPagedByCoachIdAsync(
            Guid coachId, BookingRequestType? type, List<BookingRequestStatus>? statuses, int page, int pageSize);
        Task<IEnumerable<BookingRequest>> GetExpiredPendingRequestsAsync();
    }
}
