using System;

namespace Intervu.Application.DTOs.Admin
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public Guid InterviewerId { get; set; }
        public Guid StudentId { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public string AIAnalysis { get; set; }
    }
}
