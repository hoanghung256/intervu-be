using Intervu.Application.DTOs.Feedback;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
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
        public Dictionary<string, string> LanguageCodes { get; set; }
        public string ProblemDescription { get; set; }
        public string ProblemShortName { get; set; }
        public object[] TestCases { get; set; }

        public RoomState()
        {
            CurrentLanguage = "javascript";
            ProblemDescription = string.Empty;
            LanguageCodes = new Dictionary<string, string>();
            ProblemShortName = string.Empty;
            TestCases = Array.Empty<object>();
        }
    }

    public class RoomManagerService
    {
        private readonly ILogger<RoomManagerService> _logger;
        private readonly ConcurrentDictionary<string, RoomState> _roomStates = new();
        private readonly ConcurrentDictionary<string, Timer> _roomTimers = new();
        private readonly TimeSpan _roomExpiryTime = TimeSpan.FromMinutes(1);
        private readonly IServiceScopeFactory _scopeFactory;

        public RoomManagerService(IServiceScopeFactory scopeFactory, ILogger<RoomManagerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Gets the state for a room, creating it if it doesn't exist.
        public async Task<RoomState> GetOrCreateRoomStateAsync(string roomId)
        {
            // If a timer exists for this room, it means someone is rejoining.
            // We should cancel the cleanup timer.
            if (_roomTimers.TryRemove(roomId, out var timer))
            {
                timer.Dispose();
                _logger.LogInformation("Re-activated room '{RoomId}' and cancelled expiry timer.", roomId);
            }
            using var scope = _scopeFactory.CreateScope();

            var getCurrentRoom = scope.ServiceProvider.GetRequiredService<IGetCurrentRoom>();
            Domain.Entities.InterviewRoom interviewRoom = await getCurrentRoom.ExecuteAsync(int.Parse(roomId));
            return _roomStates.GetOrAdd(roomId, _ =>
            {
                _logger.LogInformation("Creating new in-memory state for room '{RoomId}'.", roomId);
                return new RoomState()
                {
                    CurrentLanguage = interviewRoom.CurrentLanguage ?? "java",
                    LanguageCodes = interviewRoom.LanguageCodes ?? new Dictionary<string, string>(),
                    ProblemDescription = interviewRoom.ProblemDescription,
                    ProblemShortName = interviewRoom.ProblemShortName,
                    TestCases = interviewRoom.TestCases
                };
            });
        }

        // Schedules a room for cleanup if it's empty.
        public void ScheduleRoomCleanup(string roomId)
        {
            var timer = new Timer(async _ =>
            {
                using var scope = _scopeFactory.CreateScope();

                var getCurrentRoom = scope.ServiceProvider.GetRequiredService<IGetCurrentRoom>();
                var updateRoom = scope.ServiceProvider.GetRequiredService<IUpdateRoom>();
                var getFeedbacks = scope.ServiceProvider.GetRequiredService<IGetFeedbacks>();
                var createFeedback = scope.ServiceProvider.GetRequiredService<ICreateFeedback>();
                _roomStates.TryGetValue(roomId, out var roomState);

                if (roomState != null)
                {
                    //Save room progress
                    var room = await getCurrentRoom.ExecuteAsync(int.Parse(roomId));
                    if (room != null)
                    {
                        room.CurrentLanguage = roomState.CurrentLanguage;
                        room.LanguageCodes = roomState.LanguageCodes;
                        room.ProblemDescription = roomState.ProblemDescription;
                        room.ProblemShortName = roomState.ProblemShortName;
                        room.TestCases = roomState.TestCases;
                        await updateRoom.ExecuteAsync(room);
                    }
                    //Create feedback
                    GetFeedbackRequest request = new GetFeedbackRequest
                    {
                        StudentId = room.StudentId.Value,
                    };
                    var feedbacks = await getFeedbacks.ExecuteAsync(request);
                    var filterFeedbacks = feedbacks.Items.Where(f => f.InterviewerId == room.InterviewerId.Value).ToList();
                    if (filterFeedbacks.Count == 0)
                    {
                        Feedback feedback = new Feedback
                        {
                            InterviewerId = room.InterviewerId.Value,
                            StudentId = room.StudentId.Value,
                            InterviewRoomId = room.Id,
                            Rating = 0,
                            Comments = "",
                            AIAnalysis = ""
                        };
                        await createFeedback.ExecuteAsync(feedback);
                    }
                }
                if (_roomStates.TryRemove(roomId, out RoomState _))
                {
                    _logger.LogInformation("Room '{RoomId}' has been inactive for {ExpiryTime} minutes and its state has been cleared.", roomId, _roomExpiryTime.Minutes);
                }
                _roomTimers.TryRemove(roomId, out var removedTimer);
                removedTimer?.Dispose();
            }, null, _roomExpiryTime, Timeout.InfiniteTimeSpan);

            _roomTimers[roomId] = timer;
            _logger.LogInformation("Room '{RoomId}' is empty. Scheduled for cleanup in {ExpiryTime} minutes.", roomId, _roomExpiryTime.Minutes);
        }
    }
}
