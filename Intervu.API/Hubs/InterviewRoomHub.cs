using Microsoft.AspNetCore.SignalR;

namespace Intervu.API.Hubs
{
	public class InterviewRoomHub : Hub
	{
        private static Dictionary<string, string> UserConnectionMap = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnectionMap[userId] = Context.ConnectionId;
                Console.WriteLine($"User {userId} connected with {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = UserConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != null)
            {
                UserConnectionMap.Remove(userId);
                Console.WriteLine($"User {userId} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string room)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, room);

			await Clients.Group(room).SendAsync("UserJoined", Context.ConnectionId);
		}

        public async Task LeaveRoom(string room)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("UserLeft", Context.ConnectionId);
        }

		public async Task SendOffer(string toUserId, string sdp)
		{
            if (UserConnectionMap.TryGetValue(toUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveOffer", Context.ConnectionId, sdp);
            }
        }

		public async Task SendAnswer(string toConnectionId, string sdp)
		{
            await Clients.Client(toConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, sdp);
        }

		public async Task SendIceCandidate(string toConnectionId, string candidate)
        {
            await Clients.Client(toConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }
    }
}
