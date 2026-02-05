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

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public CoachAvailabilityStatus Status { get; set; }

        // If Status is Reserved, this field represents the User.Id who reserved this slot
        public Guid? ReservingForUserId { get; set; }

        public CoachProfile? CoachProfile { get; set; }

        public CandidateProfile? ReservingForUser { get; set; }

        public ICollection<InterviewBookingTransaction> InterviewBookingTransactions { get; set; } = [];

        public bool IsUserAbleToBook(Guid userId)
        {
            return Status == CoachAvailabilityStatus.Available || (Status == CoachAvailabilityStatus.Reserved && ReservingForUserId == userId);
        }
    }
}
