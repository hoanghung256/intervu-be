namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: send a notification to a specific user.</summary>
    public class CreateNotificationRequestDto
    {
        public Guid UserId { get; set; }
        public Domain.Entities.Constants.NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
