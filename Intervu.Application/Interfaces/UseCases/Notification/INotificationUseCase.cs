using Intervu.Application.DTOs.Notification;

namespace Intervu.Application.Interfaces.UseCases.Notification
{
    public interface INotificationUseCase
    {
        Task CreateAsync(Guid userId, Domain.Entities.Constants.NotificationType type, string title, string message,
            string? actionUrl = null, Guid? referenceId = null);

        Task CreateForMultipleUsersAsync(List<Guid> userIds, Domain.Entities.Constants.NotificationType type,
            string title, string message, string? actionUrl = null, Guid? referenceId = null);

        /// <summary>Broadcast to all users in the system (inserts per user in batches).</summary>
        Task BroadcastToAllAsync(Domain.Entities.Constants.NotificationType type,
            string title, string message, string? actionUrl = null);

        /// <summary>Broadcast to all users with a specific role.</summary>
        Task BroadcastToRoleAsync(string role, Domain.Entities.Constants.NotificationType type,
            string title, string message, string? actionUrl = null);

        Task<NotificationListResponseDto> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);

        Task<int> GetUnreadCountAsync(Guid userId);

        Task MarkAsReadAsync(Guid notificationId, Guid currentUserId);

        Task MarkAllAsReadAsync(Guid userId);

        // Called by Hangfire delayed job
        Task SendInterviewReminderAsync(Guid roomId);
    }
}