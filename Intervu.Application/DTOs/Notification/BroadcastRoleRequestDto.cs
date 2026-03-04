namespace Intervu.Application.DTOs.Notification
{
    /// <summary>Admin: broadcast to all users in a given role.</summary>
    public class BroadcastRoleRequestDto
    {
        /// <summary>Role name: "Candidate", "Coach", or "Admin".</summary>
        public string Role { get; set; } = string.Empty;
        public Domain.Entities.Constants.NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
    }
}
