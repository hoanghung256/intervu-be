using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class InterviewRoomDto
    {
        public Guid Id { get; set; }
        public Guid? CandidateId { get; set; }
        public string? CandidateName { get; set; }
        public Guid? CoachId { get; set; }
        public string? CoachName { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public int? DurationMinutes { get; set; }
        public string? VideoCallRoomUrl { get; set; }
        public string? CurrentLanguage { get; set; }
        public string? ProblemDescription { get; set; }
        public string? ProblemShortName { get; set; }
        public InterviewRoomStatus Status { get; set; }
        public bool IsEvaluationCompleted { get; set; }
        public double? Score { get; set; }
        public int RescheduleAttemptCount { get; set; }
        public InterviewRoomType Type { get; set; }

        // --- New booking context fields ---

        /// <summary>
        /// FK to BookingRequest (for Flow B/C rooms)
        /// </summary>
        public Guid? BookingRequestId { get; set; }

        /// <summary>
        /// The interview service type for this room
        /// </summary>
        public string? InterviewTypeName { get; set; }

        /// <summary>
        /// Target interview level
        /// </summary>
        public AimLevel? AimLevel { get; set; }

        /// <summary>
        /// Round number for JD multi-round interviews (Flow C)
        /// </summary>
        public int? RoundNumber { get; set; }
        
        /// <summary>
        /// Indicates if this interview has a pending reschedule request
        /// </summary>
        public bool HasPendingReschedule { get; set; }
        
        /// <summary>
        /// Indicates if this interview can be rescheduled (based on time and attempt count)
        /// </summary>
        public bool CanReschedule { get; set; }
    }
}
