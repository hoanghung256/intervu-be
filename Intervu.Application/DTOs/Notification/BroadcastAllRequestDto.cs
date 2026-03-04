namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: broadcast to all users in the system.</summary>
    public class BroadcastAllRequestDto
    {
        public Domain.Entities.Constants.NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
    }
}
