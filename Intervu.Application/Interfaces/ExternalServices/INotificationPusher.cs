using Intervu.Application.DTOs.Notification;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface INotificationPusher
    {
        Task PushToUserAsync(Guid userId, NotificationDto notification);
    }
}
