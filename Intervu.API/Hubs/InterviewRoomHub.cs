using Intervu.Application.Services;
using Intervu.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.API.Utils;
using Intervu.Domain.Entities;
using Intervu.Application.Interfaces.UseCases.Audit;
using Intervu.Domain.Entities.Constants;

namespace Intervu.API.Hubs
{
    public class InterviewRoomHub : Hub
    {
        private readonly CodeExecutionService _codeExecutionService;
        private readonly ILogger<InterviewRoomHub> _logger;
        private readonly RoomManagerService _roomManager;
        private readonly IReadOnlyDictionary<string, ICodeGenerationService> _codeGenerationServices;
        private readonly InterviewRoomCache _cache;
        private readonly IAddAuditLogEntry _addAuditLogEntry;

        // A static dictionary to track connections per room
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _roomConnections = new();

        private static readonly ConcurrentDictionary<string, string> UserConnectionMap = new();

        public InterviewRoomHub(CodeExecutionService codeExecutionService,
            ILogger<InterviewRoomHub> logger,
            RoomManagerService roomManager,
            IEnumerable<ICodeGenerationService> codeGenerationServices,
            InterviewRoomCache cache,
            IAddAuditLogEntry addAuditLogEntry)
        {
            _codeExecutionService = codeExecutionService;
            _logger = logger;
            _roomManager = roomManager;
            _codeGenerationServices = codeGenerationServices.ToDictionary(s => s.Language, StringComparer.OrdinalIgnoreCase);
            _cache = cache;
            _addAuditLogEntry = addAuditLogEntry;
        }



        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
                if (!string.IsNullOrEmpty(userId))
                {
                    // Overwrite any stale entry from a previous session
                    UserConnectionMap[userId] = Context.ConnectionId;
                    _logger.LogInformation("User {UserId} connected with connectionId {ConnectionId}", userId, Context.ConnectionId);
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            UserRole role = (UserRole) int.Parse(Context.GetHttpContext()?.Request.Query["role"].ToString());

            // Remove stale entry from the userId → connectionId map
            if (!string.IsNullOrEmpty(userId))
            {
                // Only remove if this specific connectionId is still the registered one
                // (a fast reconnect could have already registered a new connectionId)
                UserConnectionMap.TryRemove(
                    new KeyValuePair<string, string>(userId, Context.ConnectionId));
            }

            // Find which room the disconnected client belonged to
            string? roomToRemoveFrom = null;
            foreach (var room in _roomConnections)
            {
                if (room.Value.TryRemove(Context.ConnectionId, out _))
                {
                    roomToRemoveFrom = room.Key;
                    break;
                }
            }

            if (roomToRemoveFrom != null)
            {
                // Log leave event directly to DB
                Guid? userGuid = Guid.TryParse(userId, out var u) ? u : null;
                var metaData = JsonSerializer.Serialize(new
                {
                    RoomId = roomToRemoveFrom,
                    Role = role,
                    ConnectionId = Context.ConnectionId,
                    Reason = "Disconnected",
                    LeaveTime = DateTime.UtcNow
                });

                await _addAuditLogEntry.ExecuteAsync(new AuditLog
                {
                    UserId = userGuid,
                    Content = $"User {userId} ({role}) disconnected from room {roomToRemoveFrom}",
                    MetaData = metaData,
                    EventType = AuditLogEventType.RoomDisconnect,
                    Timestamp = DateTime.UtcNow
                });

                // Clean up stale media states for the departing peer
                await _roomManager.RemovePeerMediaState(roomToRemoveFrom, Context.ConnectionId);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomToRemoveFrom);
                // Notify only the remaining participants, not the disconnecting client
                await Clients.Group(roomToRemoveFrom).SendAsync("UserLeft", Context.ConnectionId);
                _logger.LogInformation("Client {ConnectionId} disconnected from room {RoomId}", Context.ConnectionId, roomToRemoveFrom);

                // If the room is now empty, schedule it for cleanup
                if (_roomConnections.TryGetValue(roomToRemoveFrom, out var connections) && connections.IsEmpty)
                {
                    _roomConnections.TryRemove(roomToRemoveFrom, out _);
                    _roomManager.ScheduleRoomCleanup(roomToRemoveFrom);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string room, string userId, UserRole role, string userName)
        {
            // Get the current state for the room (creates it if it doesn't exist)
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(room);

            // Send the full state (code, language, camera states) to the late-joiner only
            await Clients.Caller.SendAsync("ReceiveFullState", roomState);

            await Groups.AddToGroupAsync(Context.ConnectionId, room);

            if (await isRoomCompleted(room)) return;

            // Log join event directly to DB
            Guid? userGuid = Guid.TryParse(userId, out var u) ? u : null;
            var metaData = JsonSerializer.Serialize(new
            {
                RoomId = room,
                UserName = userName,
                Role = role.ToString(),
                ConnectionId = Context.ConnectionId,
                JoinTime = DateTime.UtcNow
            });

            await _addAuditLogEntry.ExecuteAsync(new AuditLog
            {
                UserId = userGuid,
                Content = $"User {userName} (ID: {userId}, Role: {role.ToString()}) joined room {room}",
                MetaData = metaData,
                EventType = AuditLogEventType.RoomJoin,
                Timestamp = DateTime.UtcNow
            });

            // Add connection to our tracker
            var roomConnectionIds = _roomConnections.GetOrAdd(room, new ConcurrentDictionary<string, bool>());

            // Capture existing peers BEFORE adding the new user so we can send the
            // correct list to the caller and notify only the existing participants.
            var existingPeers = roomConnectionIds.Keys.ToList();

            // Add the new user
            roomConnectionIds.TryAdd(Context.ConnectionId, true);

            // Tell EXISTING participants that a new peer has arrived (not the new user itself)
            await Clients.OthersInGroup(room).SendAsync("UserJoined", Context.ConnectionId);

            // Tell the new user who is already in the room
            await Clients.Caller.SendAsync("ExistingPeers", existingPeers);

            _logger.LogInformation("Client {ConnectionId} joined room {RoomId}", Context.ConnectionId, room);
        }

        public async Task LeaveRoom(string room, string userId, UserRole role, string userName)
        {
            _logger.LogInformation("Client {ConnectionId} leave room {RoomId}", Context.ConnectionId, room);

            // Log leave event directly to DB
            Guid? userGuid = Guid.TryParse(userId, out var u) ? u : null;
            var metaData = JsonSerializer.Serialize(new
            {
                RoomId = room,
                UserName = userName,
                Role = role.ToString(),
                ConnectionId = Context.ConnectionId,
                Reason = "Explicitly Left",
                LeaveTime = DateTime.UtcNow
            });

            await _addAuditLogEntry.ExecuteAsync(new AuditLog
            {
                UserId = userGuid,
                Content = $"User {userName} (ID: {userId}, Role: {role.ToString()}) left room {room}",
                MetaData = metaData,
                EventType = AuditLogEventType.RoomLeave,
                Timestamp = DateTime.UtcNow
            });

            // Clean up stale media states for the departing peer
            await _roomManager.RemovePeerMediaState(room, Context.ConnectionId);

            if (_roomConnections.TryGetValue(room, out var connections))
            {
                connections.TryRemove(Context.ConnectionId, out _);
                // If the room is now empty, schedule it for cleanup
                if (connections.IsEmpty)
                {
                    _roomConnections.TryRemove(room, out _);
                    _roomManager.ScheduleRoomCleanup(room);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("UserLeft", Context.ConnectionId);
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
        public async Task SendCode(string room, string code, string language)
        {
            if (await isRoomCompleted(room)) return;
            _logger.LogInformation("Code updated for room {RoomId}", room);
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(room);

            // Ensure we're updating the code for the correct language
            if (roomState.LanguageCodes.ContainsKey(language))
            {
                roomState.LanguageCodes[language] = code;
            }
            else
            {
                roomState.LanguageCodes.Add(language, code);
            }

            // Send only the updated code and its language to others
            await Clients.OthersInGroup(room).SendAsync("ReceiveCode", code, language);
        }

        /// <summary>
        /// Broadcasts the selected programming language and initial code to other users in the room.
        /// </summary>
        public async Task SendLanguage(string roomId, string language)
        {
            // Update the state in the manager
            if (await isRoomCompleted(roomId)) return;
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            _logger.LogInformation("Language updated for room {RoomId}", roomId);
            roomState.CurrentLanguage = language;

            // Get the code for the new language. If it doesn't exist, send an empty string.
            // The frontend will handle populating it with example code.
            var codeForNewLang = roomState.LanguageCodes.ContainsKey(language)
                ? roomState.LanguageCodes[language]
                : string.Empty;

            await Clients.OthersInGroup(roomId).SendAsync("ReceiveLanguage", language, codeForNewLang);
        }

        /// <summary>
        /// Executes the provided code and sends the result back to all clients in the room.
        /// </summary>
        /// <param name="roomId">The ID of the interview room.</param>
        /// <param name="code">The source code to execute.</param>
        /// <param name="language">The programming language of the code.</param>
        public async Task RunCode(string roomId, string code, string language)
        {
            _logger.LogInformation("Run Code for room {RoomId}", roomId);
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);

            // If a shortName exists, run in "Test Case Mode"
            if (!string.IsNullOrEmpty(roomState.ProblemShortName) &&
             roomState.TestCases.Length > 0 &&
             _codeGenerationServices.TryGetValue(language, out var codeGenService))
            {
                var allTestResults = new List<TestResult>();

                for (int i = 0; i < roomState.TestCases.Length; i++)
                {
                    var testCaseElement = (JsonElement)roomState.TestCases[i];
                    string harnessCode = codeGenService.GenerateTestHarness(code, roomState.ProblemShortName, testCaseElement);

                    if (string.IsNullOrEmpty(harnessCode))
                    {
                        _logger.LogWarning("Harness code generation failed for language {Language}.", language);
                        // If harness generation fails or language not supported for testing, fallback to simple execution
                        var simpleOutput = await _codeExecutionService.SendRequestAsync(code, language.Equals("c++") ? "cpp" : language);
                        await Clients.Group(roomId).SendAsync("ReceiveExecutionResult", simpleOutput);
                        return;
                    }

                    var executionResult = await _codeExecutionService.SendRequestAsync(harnessCode, language.Equals("c++") ? "cpp" : language);
                    var actualOutput = (executionResult.stdout ?? executionResult.stderr).Trim();
                    var expectedOutputs = testCaseElement.GetProperty("expectedOutputs").EnumerateArray().Select(e => e.GetString()?.Trim() ?? "").ToArray();

                    bool passed;
                    JsonElement? actualJson = null;
                    try
                    {
                        // Try to parse the actual output as a JSON document
                        actualJson = JsonDocument.Parse(actualOutput).RootElement;
                    }
                    catch (JsonException) { /* actualOutput is not a valid JSON document, actualJson remains null */ }

                    passed = false;
                    foreach (var expected in expectedOutputs)
                    {
                        if (actualJson.HasValue) // If actual output is valid JSON
                        {
                            try
                            {
                                var expectedJson = JsonDocument.Parse(expected).RootElement;
                                _logger.LogInformation("Expected: " + expectedJson);
                                _logger.LogInformation("Actual: " + actualJson.Value);
                                if (new JsonElementComparer().Equals(actualJson.Value, expectedJson))
                                {
                                    passed = true;
                                    break;
                                }
                            }
                            catch (JsonException) { /* One of the expected outputs might not be valid JSON, ignore it */ }
                        }
                        else // Fallback for plain string comparison
                        {
                            // Unquote the expected string if it's a string literal
                            string comparableExpected = expected;
                            if (comparableExpected.Length >= 2 && comparableExpected.StartsWith("\"") && comparableExpected.EndsWith("\""))
                            {
                                comparableExpected = comparableExpected.Substring(1, comparableExpected.Length - 2);
                            }

                            if (actualOutput == comparableExpected)
                            {
                                passed = true;
                                break;
                            }
                        }
                    }

                    var testResult = new TestResult
                    {
                        TestCaseIndex = i,
                        Passed = passed,
                        ActualOutput = actualOutput,
                        ExpectedOutput = expectedOutputs,
                        InputSummary = string.Join(", ", testCaseElement.GetProperty("inputs").EnumerateArray().Select(p => $"{p.GetProperty("name").GetString()} = {p.GetProperty("value").GetString()}")),
                        ExecutionTime = executionResult.executionTime
                    };
                    allTestResults.Add(testResult);
                    _logger.LogInformation("HarnessCode: " + harnessCode);
                }

                // Send the structured test results back to all clients
                await Clients.Group(roomId).SendAsync("ReceiveTestResults", allTestResults);
            }
            else // Fallback to "Playground Mode"
            {
                var output = await _codeExecutionService.SendRequestAsync(code, language.Equals("c++") ? "cpp" : language);
                await Clients.Group(roomId).SendAsync("ReceiveExecutionResult", output);
            }
        }

        /// <summary>
        /// Receives a problem description and test cases and broadcasts them to others in the room.
        /// </summary>
        public async Task SendProblem(string roomId, string description, string shortName, object[] testCases)
        {
            if (await isRoomCompleted(roomId)) return;
            // Update the state in the manager
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            roomState.ProblemDescription = description;
            roomState.ProblemShortName = shortName;
            roomState.TestCases = testCases;

            _logger.LogInformation("Problem updated for room {RoomId}", roomId);

            //_logger.LogInformation("Descrip: " + description.ToString());
            _logger.LogInformation("testCases: " + string.Join(", ", testCases));
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveProblem", description, shortName, testCases);

            if (!string.IsNullOrEmpty(shortName) && testCases.Length > 0 &&
             _codeGenerationServices.TryGetValue(roomState.CurrentLanguage, out var codeGenService))
            {
                // For now, we assume Java. This can be expanded later.
                var generatedCode = codeGenService.GenerateTemplate(shortName, testCases);
                if (!string.IsNullOrEmpty(generatedCode))
                {
                    // Update the code in the room state as well
                    roomState.LanguageCodes[roomState.CurrentLanguage] = generatedCode;
                    // Send the new code to all clients in the room (include language so the frontend applies it to the correct slot)
                    await Clients.Group(roomId).SendAsync("ReceiveCode", generatedCode, roomState.CurrentLanguage);
                }
            }
        }

        /// <summary>
        /// Called by a client when its camera is toggled on or off.
        /// Persists the state in RoomState (for late-joiners) and broadcasts
        /// to all other participants in the room.
        /// </summary>
        public async Task SendCameraState(string roomId, bool isOn)
        {
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            roomState.PeerCameraStates[Context.ConnectionId] = isOn;
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveCameraState", Context.ConnectionId, isOn);
        }

        /// <summary>
        /// Broadcasts whiteboard scene changes to other participants in the room.
        /// Also persists the latest state in-memory for late-joiners.
        /// </summary>
        public async Task SendWhiteboardState(string roomId, string elementsJson, string appStateJson)
        {
            if (await isRoomCompleted(roomId)) return;
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            roomState.WhiteboardElements = elementsJson;
            // appStateJson is accepted for backward compat but intentionally not stored/broadcast.
            // Each peer keeps their own tool/color selection locally.

            await Clients.OthersInGroup(roomId)
                .SendAsync("ReceiveWhiteboardState", elementsJson);
        }

        /// <summary>
        /// Called by a client when its microphone is toggled on or off.
        /// Persists the state in RoomState (for late-joiners) and broadcasts
        /// to all other participants in the room.
        /// </summary>
        public async Task SendMicState(string roomId, bool isOn)
        {
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            roomState.PeerMicStates[Context.ConnectionId] = isOn;
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveMicState", Context.ConnectionId, isOn);
        }

        /// <summary>
        /// Broadcasts live transcript updates (final or interim) to other participants.
        /// </summary>
        public async Task SendTranscript(string roomId, string? final, string? interim, UserRole role)
        {
            //if (await isRoomCompleted(roomId)) return;
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveTranscript", Context.ConnectionId, final, interim, role);
        }

        public async Task<bool> isRoomCompleted(string roomId)
        {
            var room = _cache.Rooms.SingleOrDefault(r => r.Id == Guid.Parse(roomId));
            if (room != null && room.Status == Domain.Entities.Constants.InterviewRoomStatus.Completed)
            {
                _logger.LogInformation("Room is completed for id: " + roomId);
                return true;
            }
            return false;
        }
    }

    public class TestResult
    {
        public bool Passed { get; set; }
        public int TestCaseIndex { get; set; }
        public string InputSummary { get; set; }
        public string ActualOutput { get; set; }
        public string[] ExpectedOutput { get; set; }
        public long ExecutionTime { get; set; }
    }
}
