using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Represents a coach's available time range (e.g., 08:00 AM to 11:00 AM).
    /// Candidates can book specific time slots within these ranges.
    /// </summary>
    public class CoachAvailability : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents AvailabilityId
        /// References CoachProfile.Id
        /// </summary>
        public Guid CoachId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public CoachAvailabilityStatus Status { get; set; }

        // Navigation
        public CoachProfile? CoachProfile { get; set; }

        public ICollection<InterviewBookingTransaction> InterviewBookingTransactions { get; set; } = [];
    }
}
