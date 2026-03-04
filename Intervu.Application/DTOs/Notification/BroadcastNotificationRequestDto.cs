namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: broadcast a notification to a specific list of users.</summary>
    public class BroadcastNotificationRequestDto
    {
        public List<Guid> UserIds { get; set; } = new();
        public Domain.Entities.Constants.NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
    }
}
