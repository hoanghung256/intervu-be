using Intervu.Application.DTOs.Notification;

namespace Intervu.Application.Interfaces.UseCases.Notification
{
    public interface INotificationUseCase
    {
        Task CreateAsync(Guid userId, Domain.Entities.Constants.NotificationType type, string title, string message,
            string? actionUrl = null, Guid? referenceId = null);

        Task CreateForMultipleUsersAsync(List<Guid> userIds, Domain.Entities.Constants.NotificationType type,
            string title, string message, string? actionUrl = null, Guid? referenceId = null);

        Task<NotificationListResponse> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);

        Task<int> GetUnreadCountAsync(Guid userId);

        Task MarkAsReadAsync(Guid notificationId);

        Task MarkAllAsReadAsync(Guid userId);

        // Called by Hangfire delayed job
        Task SendInterviewReminderAsync(Guid roomId);
    }
}
