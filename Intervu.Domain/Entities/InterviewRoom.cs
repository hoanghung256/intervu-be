using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class InterviewRoom : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents InterviewRoomId
        /// </summary>
        public Guid? CandidateId { get; set; }

        public Guid? CoachId { get; set; }

        public Guid? TransactionId { get; set; }

        /// <summary>
        /// Flow A: links to the availability range. Nullable for Flow B/C.
        /// </summary>
        public Guid? CurrentAvailabilityId { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public int? DurationMinutes { get; set; }

        public string? VideoCallRoomUrl { get; set; }

        public string? CurrentLanguage { get; set; }

        public Dictionary<string, string>? LanguageCodes { get; set; }

        public string? ProblemDescription { get; set; }

        public string? ProblemShortName { get; set; }

        public object[]? TestCases { get; set; }

        /// <summary>
        /// Scheduled, Completed, Cancelled, No-Show
        /// </summary>
        public InterviewRoomStatus Status { get; set; }

        public int RescheduleAttemptCount { get; set; } = 0;

        // --- New booking context fields ---

        /// <summary>
        /// FK to BookingRequest (for Flow B/C rooms)
        /// </summary>
        public Guid? BookingRequestId { get; set; }

        /// <summary>
        /// FK to CoachInterviewService — the service type for this room
        /// </summary>
        public Guid? CoachInterviewServiceId { get; set; }

        /// <summary>
        /// Target interview level (Junior, MidLevel, Senior, TeamLeader)
        /// </summary>
        public AimLevel? AimLevel { get; set; }

        /// <summary>
        /// Which round number this room belongs to (for JD multi-round Flow C)
        /// </summary>
        public int? RoundNumber { get; set; }

        // Navigation Properties
        public InterviewBookingTransaction? Transaction { get; set; }
        
        public CoachAvailability? CurrentAvailability { get; set; }

        public BookingRequest? BookingRequest { get; set; }

        public CoachInterviewService? CoachInterviewService { get; set; }
        
        public ICollection<InterviewRescheduleRequest>? RescheduleRequests { get; set; }

        public bool IsAvailableForReschedule()
        {
            if (ScheduledTime == null) return false;

            var timeUntilInterview = ScheduledTime.Value - DateTime.UtcNow;
            if (timeUntilInterview < TimeSpan.FromHours(12))
            {
                return false;
            }
            // Limit 1 reschedule attempt per interview
            if (RescheduleAttemptCount >= 1)
            {
                return false;
            }
            return true;
        }
    }
}
