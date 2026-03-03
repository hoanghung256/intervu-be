using Intervu.API.Hubs;
using Intervu.Application.DTOs.Notification;
using Intervu.Application.Interfaces.ExternalServices;
using Microsoft.AspNetCore.SignalR;

namespace Intervu.API.Services
{
    public class SignalRNotificationPusher : INotificationPusher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PushToUserAsync(Guid userId, NotificationDto notification)
        {
            await _hubContext.Clients.Group(userId.ToString())
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
