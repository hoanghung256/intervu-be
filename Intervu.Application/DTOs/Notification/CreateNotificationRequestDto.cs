using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: send a notification to a specific user.</summary>
    public class CreateNotificationRequestDto
    {
        public Guid UserId { get; set; }
        public Domain.Entities.Constants.NotificationType Type { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        public Guid? ReferenceId { get; set; }
    }
}
