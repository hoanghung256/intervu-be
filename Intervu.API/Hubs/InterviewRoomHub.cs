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

namespace Intervu.API.Hubs
{
    public class InterviewRoomHub : Hub
    {
        private readonly CodeExecutionService _codeExecutionService;
        private readonly ILogger<InterviewRoomHub> _logger;
        private readonly RoomManagerService _roomManager;
        private readonly IReadOnlyDictionary<string, ICodeGenerationService> _codeGenerationServices;

        // A static dictionary to track connections per room
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _roomConnections = new();

        private static Dictionary<string, string> UserConnectionMap = new();
        private static Dictionary<string, HashSet<string>> RoomUsers = new();

        public InterviewRoomHub(CodeExecutionService codeExecutionService,
            ILogger<InterviewRoomHub> logger,
            RoomManagerService roomManager,
            IEnumerable<ICodeGenerationService> codeGenerationServices)
        {
            _codeExecutionService = codeExecutionService;
            _logger = logger;
            _roomManager = roomManager;
            _codeGenerationServices = codeGenerationServices.ToDictionary(s => s.Language, StringComparer.OrdinalIgnoreCase);
        }



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
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomToRemoveFrom);
                await Clients.Group(roomToRemoveFrom).SendAsync("UserLeft", Context.ConnectionId);
                _logger.LogInformation("Client {ConnectionId} disconnected from room {RoomId}", Context.ConnectionId, roomToRemoveFrom);

                // If the room is now empty, schedule it for cleanup
                if (_roomConnections.TryGetValue(roomToRemoveFrom, out var connections) && connections.IsEmpty)
                {
                    // Also remove the room key from the top-level dictionary to keep it clean
                    _roomConnections.TryRemove(roomToRemoveFrom, out _);
                    _roomManager.ScheduleRoomCleanup(roomToRemoveFrom);
                }
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

            //var userId = UserConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            //if (userId != null)
            //{
            //    UserConnectionMap.Remove(userId);
            //    Console.WriteLine($"User {userId} disconnected");
            //}

            //await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string room)
        {
            // Add connection to our tracker
            var roomConnectionIds = _roomConnections.GetOrAdd(room, new ConcurrentDictionary<string, bool>());
            roomConnectionIds.TryAdd(Context.ConnectionId, true);

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

            // Get the current state for the room
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(room);

            // Send the entire state to the newly joined user. This is correct.
            await Clients.Caller.SendAsync("ReceiveFullState", roomState);

            await Clients.Group(room).SendAsync("UserJoined", Context.ConnectionId);
            _logger.LogInformation("Client {ConnectionId} joined room {RoomId}", Context.ConnectionId, room);
        }

        public async Task LeaveRoom(string room)
        {
            _logger.LogInformation("Client {ConnectionId} leave room {RoomId}", Context.ConnectionId, room);
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
                    // Send the new code to all clients in the room
                    await Clients.Group(roomId).SendAsync("ReceiveCode", generatedCode);
                }
            }
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
