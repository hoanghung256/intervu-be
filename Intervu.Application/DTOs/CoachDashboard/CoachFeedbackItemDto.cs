namespace Intervu.Application.DTOs.CoachDashboard
{
    public class CoachFeedbackItemDto
    {
        public int Rating { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
