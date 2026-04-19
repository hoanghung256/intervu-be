using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities.Projections
{
    public class AdminBroadcastLogEntry
    {
        public DateTime CreatedAt { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public int TotalRecipients { get; set; }
        public int CandidateRecipients { get; set; }
        public int CoachRecipients { get; set; }
        public int AdminRecipients { get; set; }
    }
}
