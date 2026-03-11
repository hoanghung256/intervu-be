using Intervu.Application.DTOs.Notification;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface INotificationPusher
    {
        /// <summary>Push to a single user's SignalR group.</summary>
        Task PushToUserAsync(Guid userId, NotificationDto notification);

        /// <summary>Push to all connected clients.</summary>
        Task PushToAllAsync(NotificationDto notification);

        /// <summary>Push to all clients in a role group (e.g. "Coach", "Candidate").</summary>
        Task PushToRoleGroupAsync(string role, NotificationDto notification);
    }
}
