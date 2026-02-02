using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class CoachAvailability : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents AvailabilityId
        /// References CoachProfile.Id
        /// </summary>
        public Guid CoachId { get; set; }

        public Guid TypeId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public InterviewFocus Focus { get; set; }

        public CoachAvailabilityStatus Status { get; set; }

        public CoachProfile? CoachProfile { get; set; }

        public ICollection<InterviewBookingTransaction> InterviewBookingTransactions { get; set; } = [];
    }
}
