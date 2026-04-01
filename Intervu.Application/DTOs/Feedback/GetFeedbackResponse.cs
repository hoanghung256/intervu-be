using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Feedback
{
    public class GetFeedbackResponse
    {
        public Guid FeedbackId { get; set; }
        public string CoachName { get; set; }

        public Guid InterviewRoomId { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public int? DurationMinutes { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        public string AIAnalysis { get; set; }
    }
}