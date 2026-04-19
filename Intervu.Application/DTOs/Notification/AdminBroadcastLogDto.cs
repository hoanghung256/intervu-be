namespace Intervu.Application.DTOs.Notification
{
    public class AdminBroadcastLogDto
    {
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public int TotalRecipients { get; set; }
        public int CandidateRecipients { get; set; }
        public int CoachRecipients { get; set; }
        public int AdminRecipients { get; set; }
    }
}
