using System;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class CreateInterviewRoomDto
    {
        [Required]
        public Guid CandidateId { get; set; }

        /// <summary>
        /// Optional: If null, creates an unscheduled room for the candidate.
        /// If provided, creates a scheduled room with the specified coach.
        /// </summary>
        public Guid? CoachId { get; set; }

        /// <summary>
        /// Optional: Scheduled time for the interview.
        /// Only used when CoachId is provided.
        /// </summary>
        public DateTime? ScheduledTime { get; set; }
    }
}
