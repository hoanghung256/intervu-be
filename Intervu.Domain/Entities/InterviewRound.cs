using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Represents a single round in a multi-round JD interview (Flow C).
    /// Each round maps to one InterviewRoom after payment is completed.
    /// 
    /// Validation rules (enforced in Application layer):
    /// 1. Round[n+1].StartTime >= Round[n].EndTime + 15 minutes
    /// 2. Behavioral rounds (IsCoding=false) must come after Technical rounds (IsCoding=true)
    /// 3. Sum(Price) == BookingRequest.TotalAmount
    /// </summary>
    public class InterviewRound : EntityAuditable<Guid>
    {
        public Guid BookingRequestId { get; set; }

        public Guid CoachInterviewServiceId { get; set; }

        /// <summary>
        /// Sequential round number: 1, 2, 3...
        /// </summary>
        public int RoundNumber { get; set; }

        public DateTime StartTime { get; set; }

        /// <summary>
        /// Computed as StartTime + CoachInterviewService.DurationMinutes
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Snapshot of price at booking time (from CoachInterviewService.Price)
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Set after payment — links to the InterviewRoom created for this round
        /// </summary>
        public Guid? InterviewRoomId { get; set; }

        // Navigation
        public BookingRequest BookingRequest { get; set; } = null!;
        public CoachInterviewService CoachInterviewService { get; set; } = null!;
        public InterviewRoom? InterviewRoom { get; set; }

        /// <summary>
        /// The consecutive 30-min CoachAvailability blocks assigned to this round.
        /// </summary>
        public ICollection<CoachAvailability> AvailabilityBlocks { get; set; } = [];
    }
}
