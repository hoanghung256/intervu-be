using Microsoft.AspNetCore.SignalR;

namespace Intervu.API.Hubs
{
	public class InterviewRoomHub : Hub
	{
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

		public async Task SendOffer(string toCOnnectionId, string sdp)
		{
			await Clients.Client(toCOnnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, sdp);
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
