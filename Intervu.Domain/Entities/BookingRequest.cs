using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Handles booking requests for all flows:
    /// Flow A (Direct) — 1 round from available slot, auto-accepted
    /// Flow B (External) — outside coach's available time
    /// Flow C (JD Multi-Round) — multiple rounds
    ///
    /// State machine:
    ///   Paid flow:  Pending → PendingForApprovalAfterPayment → Accepted (rooms created)
    ///                                                        → Rejected (refund issued)
    ///   Free flow:  Pending → Accepted (rooms created immediately, no payment step)
    ///   Expiry:     Pending → Expired (no payment received in time)
    ///               PendingForApprovalAfterPayment → Expired (coach did not respond in time, refund issued)
    ///   Cancel:     Pending | PendingForApprovalAfterPayment | Accepted → Cancelled
    /// </summary>
    public class BookingRequest : EntityAuditable<Guid>
    {
        public Guid CandidateId { get; set; }
        public Guid CoachId { get; set; }

        public BookingRequestType Type { get; set; }
        public BookingRequestStatus Status { get; set; }

        // --- External Booking (Flow B) fields ---

        /// <summary>
        /// The CoachInterviewService chosen by the candidate (determines type + price + duration).
        /// Used by both Flow B (single service) and as default for Flow C rounds.
        /// </summary>
        public Guid? CoachInterviewServiceId { get; set; }

        /// <summary>
        /// Candidate's desired start time (for Flow B — outside available ranges)
        /// </summary>
        public DateTime? RequestedStartTime { get; set; }

        public AimLevel? AimLevel { get; set; }

        // --- JD Interview (Flow C) fields ---

        /// <summary>
        /// URL of the uploaded Job Description file
        /// </summary>
        public string? JobDescriptionUrl { get; set; }

        /// <summary>
        /// URL of the CV — either newly uploaded or picked from CandidateProfile
        /// </summary>
        public string? CVUrl { get; set; }

        // --- Coach response ---

        public string? RejectionReason { get; set; }
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Auto-expire pending requests after this time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // --- Payment ---

        /// <summary>
        /// Calculated total price (sum of all round prices for Flow C, 
        /// or single service price for Flow B)
        /// </summary>
        public int TotalAmount { get; set; }

        // Navigation
        public CandidateProfile Candidate { get; set; } = null!;
        public CoachProfile Coach { get; set; } = null!;
        public CoachInterviewService? CoachInterviewService { get; set; }
        public ICollection<InterviewRound> Rounds { get; set; } = [];
        public ICollection<InterviewBookingTransaction> Transactions { get; set; } = [];
    }
}
