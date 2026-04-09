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
        Task<IEnumerable<BookingRequest>> GetExpiredPaidRequestsAsync();

        /// <summary>
        /// Returns (StartTime, EndTime) intervals for all rounds belonging to
        /// active (Pending/Accepted/Paid) booking requests for the given coach
        /// that overlap with [rangeStart, rangeEnd).
        /// </summary>
        Task<List<(DateTime Start, DateTime End)>> GetActiveRoundsByCoachAsync(
            Guid coachId, DateTime rangeStart, DateTime rangeEnd);

        Task<List<(DateTime Start, DateTime End)>> GetConfirmedBookingsForCoachAsync(Guid coachId, int month, int year);
        Task<List<InterviewRound>> GetConfirmedBookingEntitiesForCoachAsync(Guid coachId, int month, int year);
    }
}
