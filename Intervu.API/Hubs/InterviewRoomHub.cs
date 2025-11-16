using Microsoft.AspNetCore.SignalR;

namespace Intervu.API.Hubs
{
    public class InterviewRoomHub : Hub
    {
        private static Dictionary<string, string> UserConnectionMap = new();
        private static Dictionary<string, HashSet<string>> RoomUsers = new();

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

            // Remove from all rooms
            foreach (var room in RoomUsers.ToList())
            {
                if (room.Value.Contains(Context.ConnectionId))
                {
                    room.Value.Remove(Context.ConnectionId);
                    if (room.Value.Count == 0)
                    {
                        RoomUsers.Remove(room.Key);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string room)
        {
            // Add to SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, room);

            // Track room membership
            if (!RoomUsers.ContainsKey(room))
            {
                RoomUsers[room] = new HashSet<string>();
            }

            // Get existing peers before adding new user
            var existingPeers = RoomUsers[room].ToList();

            // Add new user to room
            RoomUsers[room].Add(Context.ConnectionId);

            // Send existing peers to the new user
            await Clients.Caller.SendAsync("ExistingPeers", existingPeers);

            // Notify existing users about new user
            await Clients.OthersInGroup(room).SendAsync("UserJoined", Context.ConnectionId);

            Console.WriteLine($"User {Context.ConnectionId} joined room {room}. Total users: {RoomUsers[room].Count}");
        }

        public async Task LeaveRoom(string room)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

            // Remove from room tracking
            if (RoomUsers.ContainsKey(room))
            {
                RoomUsers[room].Remove(Context.ConnectionId);
                if (RoomUsers[room].Count == 0)
                {
                    RoomUsers.Remove(room);
                }
            }

            await Clients.Group(room).SendAsync("UserLeft", Context.ConnectionId);
            Console.WriteLine($"User {Context.ConnectionId} left room {room}");
        }

        public async Task SendOffer(string toConnectionId, string sdp)
        {
            await Clients.Client(toConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, sdp);
        }

        public async Task SendAnswer(string toConnectionId, string sdp)
        {
            await Clients.Client(toConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, sdp);
        }

        public async Task SendIceCandidate(string toConnectionId, string candidate)
        {
            await Clients.Client(toConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        /// <summary>
        /// Broadcasts code changes to other users in the same room.
        /// </summary>
        public async Task SendCode(string room, string code)
        {
            await Clients.OthersInGroup(room).SendAsync("ReceiveCode", code);
        }

        /// <summary>
        /// Broadcasts the selected programming language and initial code to other users in the room.
        /// </summary>
        public async Task SendLanguage(string roomId, string language, string initialCode)
        {
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveLanguage", language, initialCode);
        }

        /// <summary>
        /// Broadcasts questions to all users in the room.
        /// </summary>
        public async Task SendQuestions(string roomId, List<string> questions)
        {
            await Clients.Group(roomId).SendAsync("ReceiveQuestions", questions);
        }

        /// <summary>
        /// Broadcasts whiteboard text to all users in the room.
        /// </summary>
        public async Task SendWhiteboardText(string roomId, string text)
        {
            await Clients.Group(roomId).SendAsync("ReceiveWhiteboardText", text);
        }
    }
}
