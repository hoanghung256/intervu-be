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
        public int RescheduleAttemptCount { get; set; }
        
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
