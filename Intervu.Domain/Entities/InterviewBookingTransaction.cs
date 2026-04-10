using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class InterviewBookingTransaction : EntityDateTracking<Guid>
    {
        // For tracking with PayOS, IDENTITY(1,1)
        public int OrderCode { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Links payment to the booking request (all flows)
        /// </summary>
        public Guid? BookingRequestId { get; set; }

        public int Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        // Navigation
        public BookingRequest? BookingRequest { get; set; }
        public User? User { get; set; }
    }
}
