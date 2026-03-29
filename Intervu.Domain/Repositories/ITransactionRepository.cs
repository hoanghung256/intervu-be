using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Repositories
{
    public interface ITransactionRepository : IRepositoryBase<InterviewBookingTransaction>
    {
        Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id);

        Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id, TransactionType type);

        Task<InterviewBookingTransaction?> Get(int orderCode, TransactionType type);

        Task<(IReadOnlyList<InterviewBookingTransaction> Items, int TotalItems)> GetListByUserAsync(
            Guid userId,
            int page,
            int pageSize,
            TransactionType? type = null,
            TransactionStatus? status = null);
        /// <summary>
        /// Checks whether any active (Created or Paid) Payment-type booking for the given coach
        /// overlaps the requested [startTime, endTime) range.
        /// </summary>
        Task<bool> HasOverlappingBookingAsync(Guid coachId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Returns all active (Created or Paid) Payment-type bookings for a coach
        /// whose booked time range overlaps [rangeStart, rangeEnd).
        /// Used by AvailabilityCalculatorService to compute free slots.
        /// </summary>
        Task<List<InterviewBookingTransaction>> GetActiveBookingsByCoachAsync(
            Guid coachId, DateTime rangeStart, DateTime rangeEnd);

        Task<List<(DateTime Start, DateTime End)>> GetConfirmedBookingsForCoachAsync(Guid coachId, int month, int year);

        Task<List<InterviewBookingTransaction>> GetConfirmedBookingEntitiesForCoachAsync(Guid coachId, int month, int year);
    }
}
