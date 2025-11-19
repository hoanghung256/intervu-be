using System;

namespace Intervu.Application.DTOs.Admin
{
    public class FeedbackDto
    {
        public int Id { get; set; }
        public int InterviewerId { get; set; }
        public int StudentId { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public string AIAnalysis { get; set; }
    }
}
