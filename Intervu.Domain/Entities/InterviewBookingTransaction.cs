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

        /// <summary>
        /// Set on per-round Payout/Earnings rows. Null for Payment rows (paid up front for the whole booking).
        /// </summary>
        public Guid? InterviewRoundId { get; set; }

        public int Amount { get; set; }

        public int? GrossAmount { get; set; }

        public int? CommissionAmount { get; set; }

        public decimal? CommissionRate { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        // Navigation
        public BookingRequest? BookingRequest { get; set; }
        public InterviewRound? InterviewRound { get; set; }
        public User? User { get; set; }
    }
}
