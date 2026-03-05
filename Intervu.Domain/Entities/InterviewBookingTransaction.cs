using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class InterviewBookingTransaction : EntityBase<Guid>
    {
        // For tracking with PayOS, IDENTITY(1,1)
        public int OrderCode { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Flow A (normal booking): links payment to the availability slot
        /// </summary>
        public Guid? CoachAvailabilityId { get; set; }

        /// <summary>
        /// Flow B & C: links payment to the booking request
        /// </summary>
        public Guid? BookingRequestId { get; set; }

        public int Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        /// <summary>
        /// Flow A: The coach who owns the availability (needed after availability is split/deleted).
        /// </summary>
        public Guid? CoachId { get; set; }

        /// <summary>
        /// Flow A: The candidate-chosen start time within the availability range.
        /// </summary>
        public DateTime? BookedStartTime { get; set; }

        /// <summary>
        /// Flow A: Duration in minutes from the CoachInterviewService.
        /// </summary>
        public int? BookedDurationMinutes { get; set; }

        // Navigation
        public CoachAvailability? CoachAvailability { get; set; }
        public BookingRequest? BookingRequest { get; set; }
        public User? User { get; set; }
    }
}
