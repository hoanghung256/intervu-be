using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Application.DTOs.Admin
{
    /// <summary>
    /// DTO for admin transaction monitoring.
    /// Only contains fields that exist on InterviewBookingTransaction
    /// and its navigated entities (User, BookingRequest → Candidate/Coach → User).
    /// </summary>
    public class AdminTransactionDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// PayOS sequential order code (human-readable reference).
        /// </summary>
        public int OrderCode { get; set; }

        /// <summary>
        /// Payment: Candidate pays platform.
        /// Payout: Platform pays Coach after interview completed.
        /// Refund: Platform returns money to Candidate.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Created / Paid / Cancel / PendingPayout
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Amount in VND (int in domain, surfaced as decimal for frontend).
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// The user who performed (owns) this transaction.
        /// For Payment/Refund → Candidate. For Payout → Coach.
        /// </summary>
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // --- Context from BookingRequest (nullable when transaction has no booking) ---

        public Guid? BookingRequestId { get; set; }

        /// <summary>Candidate involved in the booking.</summary>
        public string? CandidateName { get; set; }

        /// <summary>Coach involved in the booking.</summary>
        public string? CoachName { get; set; }

        /// <summary>Total amount of the original booking request.</summary>
        public int? BookingTotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
