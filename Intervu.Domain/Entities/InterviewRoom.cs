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

        // Navigation Properties
        public ICollection<InterviewRescheduleRequest>? RescheduleRequests { get; set; }
    }
}
