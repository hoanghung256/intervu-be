using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Intervu.API.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.AddOrUpdate(
                    userId,
                    _ => new HashSet<string> { Context.ConnectionId },
                    (_, set) => { lock (set) { set.Add(Context.ConnectionId); } return set; });

                // Group by userId for targeted push
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            // Group by role for role-based broadcast
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(role))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Find and remove this connectionId from all user entries
            foreach (var kvp in _userConnections)
            {
                lock (kvp.Value)
                {
                    if (kvp.Value.Remove(Context.ConnectionId))
                    {
                        // Remove userId group
                        Groups.RemoveFromGroupAsync(Context.ConnectionId, kvp.Key).GetAwaiter().GetResult();

                        // Clean up empty entries
                        if (kvp.Value.Count == 0)
                            _userConnections.TryRemove(kvp.Key, out _);

                        break;
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
