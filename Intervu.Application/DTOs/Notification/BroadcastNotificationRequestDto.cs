using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: broadcast a notification to a specific list of users.</summary>
    public class BroadcastNotificationRequestDto
    {
        public List<Guid> UserIds { get; set; } = new();
        public Domain.Entities.Constants.NotificationType Type { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }
    }
}
