using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: broadcast to all users in the system.</summary>
    public class BroadcastAllRequestDto
    {
        public Domain.Entities.Constants.NotificationType Type { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }
    }
}
