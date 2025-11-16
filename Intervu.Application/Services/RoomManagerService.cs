using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Services
{
    public class RoomState
    {
        public string CurrentLanguage { get; set; }
        public string CurrentCode { get; set; }
        public string ProblemDescription { get; set; }
        public string ProblemShortName { get; set; }
        public object[] TestCases { get; set; }

        public RoomState(string initialLanguage, string initialCode)
        {
            CurrentLanguage = initialLanguage;
            CurrentCode = initialCode;
            ProblemDescription = "The problem description will appear here.";
            ProblemShortName = string.Empty;
            TestCases = new object[0];
        }
    }

    public class RoomManagerService
    {
        private readonly ILogger<RoomManagerService> _logger;
        private readonly ConcurrentDictionary<string, RoomState> _roomStates = new();
        private readonly ConcurrentDictionary<string, Timer> _roomTimers = new();
        private readonly TimeSpan _roomExpiryTime = TimeSpan.FromMinutes(5);

        public RoomManagerService(ILogger<RoomManagerService> logger)
        {
            _logger = logger;
        }

        // Gets the state for a room, creating it if it doesn't exist.
        public RoomState GetOrCreateRoomState(string roomId)
        {
            // If a timer exists for this room, it means someone is rejoining.
            // We should cancel the cleanup timer.
            if (_roomTimers.TryRemove(roomId, out var timer))
            {
                timer.Dispose();
                _logger.LogInformation("Re-activated room '{RoomId}' and cancelled expiry timer.", roomId);
            }

            return _roomStates.GetOrAdd(roomId, _ =>
            {
                _logger.LogInformation("Creating new in-memory state for room '{RoomId}'.", roomId);
                // Default state for a new room
                return new RoomState("java", "import java.util.*;\n\npublic class Main {\n    public static void main(String[] args) {\n      System.out.println(\"Hello, World!\");\n  }\n}");
            });
        }

        // Schedules a room for cleanup if it's empty.
        public void ScheduleRoomCleanup(string roomId)
        {
            var timer = new Timer(_ =>
            {
                if (_roomStates.TryRemove(roomId, out RoomState _))
                {
                    _logger.LogInformation("Room '{RoomId}' has been inactive for 5 minutes and its state has been cleared.", roomId);
                }
                _roomTimers.TryRemove(roomId, out var removedTimer);
                removedTimer?.Dispose();
            }, null, _roomExpiryTime, Timeout.InfiniteTimeSpan);

            _roomTimers[roomId] = timer;
            _logger.LogInformation("Room '{RoomId}' is empty. Scheduled for cleanup in 5 minutes.", roomId);
        }
    }
}
