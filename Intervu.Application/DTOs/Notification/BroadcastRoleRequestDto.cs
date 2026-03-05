using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: broadcast to all users in a given role.</summary>
    public class BroadcastRoleRequestDto
    {
        /// <summary>Role name: "Candidate", "Coach", or "Admin".</summary>
        [Required, MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        public Domain.Entities.Constants.NotificationType Type { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }
    }
}
