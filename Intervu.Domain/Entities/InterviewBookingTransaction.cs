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

        // Navigation
        public CoachAvailability? CoachAvailability { get; set; }
        public BookingRequest? BookingRequest { get; set; }
        public User? User { get; set; }
    }
}
