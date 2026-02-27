using Intervu.Application.DTOs.Feedback;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
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
        private readonly ConcurrentDictionary<string, Timer> _periodicSaveTimers = new();
        private readonly ConcurrentDictionary<string, Timer> _roomTimers = new();
        private readonly TimeSpan _roomExpiryTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _periodicSaveInterval = TimeSpan.FromSeconds(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly double value;
        private readonly string unit;
        private readonly InterviewRoomCache _cache;

        public RoomManagerService(IServiceScopeFactory scopeFactory, ILogger<RoomManagerService> logger, InterviewRoomCache cache)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _cache = cache;
            (value, unit) = _roomExpiryTime switch
            {
                { Seconds: > 0 } when _roomExpiryTime.Seconds == _roomExpiryTime.TotalSeconds => (_roomExpiryTime.TotalSeconds, "seconds"),
                { Minutes: > 0 } when _roomExpiryTime.Minutes == _roomExpiryTime.TotalMinutes => (_roomExpiryTime.TotalMinutes, "minutes"),
                { Hours: > 0 } when _roomExpiryTime.Hours == _roomExpiryTime.TotalHours => (_roomExpiryTime.TotalHours, "hours"),
                _ => (_roomExpiryTime.TotalSeconds, "seconds")
            };
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

            _periodicSaveTimers.GetOrAdd(roomId, _ =>
            {
                _logger.LogInformation("Starting periodic save timer for room '{RoomId}'.", roomId);
                return new Timer(async state =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var updateRoom = scope.ServiceProvider.GetRequiredService<IUpdateRoom>();
                    if (_roomStates.TryGetValue(roomId, out var roomState))
                    {
                        var roomGuid = Guid.Parse(roomId);
                        InterviewRoom room = _cache.Rooms.SingleOrDefault(r => r.Id == roomGuid);
                        if (room != null)
                        {
                            room.CurrentLanguage = roomState.CurrentLanguage;
                            room.LanguageCodes = roomState.LanguageCodes;
                            room.ProblemDescription = roomState.ProblemDescription;
                            room.ProblemShortName = roomState.ProblemShortName;
                            room.TestCases = roomState.TestCases;
                            await updateRoom.ExecuteAsync(room);
                            _logger.LogInformation("Periodically saved data for room '{RoomId}'.", roomId);
                        }
                    }
                }, null, _periodicSaveInterval, _periodicSaveInterval);
            });
            //using var scope = _scopeFactory.CreateScope();

            //var getCurrentRoom = scope.ServiceProvider.GetRequiredService<IGetCurrentRoom>();
            //Domain.Entities.InterviewRoom interviewRoom = await getCurrentRoom.ExecuteAsync(int.Parse(roomId));
            var roomGuid = Guid.Parse(roomId);
            InterviewRoom interviewRoom = _cache.Rooms.SingleOrDefault(r => r.Id == roomGuid);
            return _roomStates.GetOrAdd(roomId, _ =>
            {
                _logger.LogInformation("Creating new in-memory state for room '{RoomId}'.", roomId);
                return new RoomState()
                {
                    CurrentLanguage = interviewRoom.CurrentLanguage ?? "java",
                    LanguageCodes = interviewRoom.LanguageCodes ?? new Dictionary<string, string>(),
                    ProblemDescription = interviewRoom.ProblemDescription ?? string.Empty,
                    ProblemShortName = interviewRoom.ProblemShortName ?? string.Empty,
                    TestCases = interviewRoom.TestCases ?? Array.Empty<object>()
                };
            });
        }

        // Schedules a room for cleanup if it's empty.
        public void ScheduleRoomCleanup(string roomId)
        {
            if (_periodicSaveTimers.TryRemove(roomId, out var periodicTimer))
            {
                periodicTimer.Dispose();
                _logger.LogInformation("Stopped periodic save timer for room '{RoomId}'.", roomId);
            }

            var timer = new Timer(async _ =>
            {
                using var scope = _scopeFactory.CreateScope();

                var updateRoom = scope.ServiceProvider.GetRequiredService<IUpdateRoom>();
                var getFeedbacks = scope.ServiceProvider.GetRequiredService<IGetFeedbacks>();
                var createFeedback = scope.ServiceProvider.GetRequiredService<ICreateFeedback>();
                // var payoutForCoach = scope.ServiceProvider.GetRequiredService<IPayoutForCoachAfterInterview>();
                
                var roomGuid = Guid.Parse(roomId);
                InterviewRoom room = _cache.Rooms.SingleOrDefault(r => r.Id == roomGuid);

                if (room != null)
                {
                    room.CurrentLanguage = room.CurrentLanguage;
                    room.LanguageCodes = room.LanguageCodes;
                    room.ProblemDescription = room.ProblemDescription;
                    room.ProblemShortName = room.ProblemShortName;
                    room.TestCases = room.TestCases;
                    room.Status = InterviewRoomStatus.Completed;
                    _logger.LogInformation("Saved data for room '{RoomId}'.", roomId);                    
                    _cache.Update(room);
                    await updateRoom.ExecuteAsync(room);
                    // await payoutForCoach.ExecuteAsync(room.Id);

                    //Create feedback
                    GetFeedbackRequest request = new GetFeedbackRequest
                    {
                        StudentId = room.CandidateId,
                    };
                    var feedbacks = await getFeedbacks.ExecuteAsync(request);
                    if (room.CoachId.HasValue && room.CandidateId.HasValue && !feedbacks.Items.Any(f => f.InterviewRoomId == room.Id))
                    {
                        Feedback feedback = new Feedback
                        {
                            CoachId = room.CoachId.Value,
                            CandidateId = room.CandidateId.Value,
                            InterviewRoomId = room.Id,
                            Rating = 0, // Default value
                            Comments = "", // Default value
                            AIAnalysis = "" // Default value
                        };
                        await createFeedback.ExecuteAsync(feedback);
                    }
                }

                if (_roomStates.TryRemove(roomId, out RoomState _))
                {
                    _logger.LogInformation("Room '{RoomId}' has been inactive for {ExpiryValue} {ExpiryUnit}, marked as complete, and its state has been cleared.", roomId, value, unit);
                }
                _roomTimers.TryRemove(roomId, out var removedTimer);
                removedTimer?.Dispose();
            }, null, _roomExpiryTime, Timeout.InfiniteTimeSpan);

            _roomTimers[roomId] = timer;
            _logger.LogInformation("Room '{RoomId}' is empty. Scheduled for cleanup in {ExpiryValue} {ExpiryUnit}.", roomId, value, unit);
        }
    }
}
