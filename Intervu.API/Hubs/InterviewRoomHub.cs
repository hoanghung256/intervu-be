using Intervu.Application.Services;
using Intervu.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.API.Utils;

namespace Intervu.API.Hubs
{
    public class InterviewRoomHub : Hub
    {
        private readonly CodeExecutionService _codeExecutionService;
        private readonly ILogger<InterviewRoomHub> _logger;
        private readonly RoomManagerService _roomManager;
        private readonly IReadOnlyDictionary<string, ICodeGenerationService> _codeGenerationServices;
        private readonly IGetCurrentRoom _getCurrentRoom;

        // A static dictionary to track connections per room
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _roomConnections = new();

        private static Dictionary<string, string> UserConnectionMap = new();
        private static Dictionary<string, HashSet<string>> RoomUsers = new();

        public InterviewRoomHub(CodeExecutionService codeExecutionService,
            ILogger<InterviewRoomHub> logger,
            RoomManagerService roomManager,
            IEnumerable<ICodeGenerationService> codeGenerationServices,
            IGetCurrentRoom getCurrentRoom)
        {
            _codeExecutionService = codeExecutionService;
            _logger = logger;
            _roomManager = roomManager;
            _codeGenerationServices = codeGenerationServices.ToDictionary(s => s.Language, StringComparer.OrdinalIgnoreCase);
            _getCurrentRoom = getCurrentRoom;
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

                if (_roomConnections.TryGetValue(roomToRemoveFrom, out var connections) && connections.IsEmpty)
                {
                    _roomConnections.TryRemove(roomToRemoveFrom, out _);
                    _roomManager.ScheduleRoomCleanup(roomToRemoveFrom);
                }
            }

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
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(room);

            await Clients.Caller.SendAsync("ReceiveFullState", roomState);

            await Groups.AddToGroupAsync(Context.ConnectionId, room);

            await Clients.Group(room).SendAsync("UserJoined", Context.ConnectionId);

            if (await isRoomCompleted(room)) return;
            
            var roomConnectionIds = _roomConnections.GetOrAdd(room, new ConcurrentDictionary<string, bool>());
            roomConnectionIds.TryAdd(Context.ConnectionId, true);

            if (!RoomUsers.ContainsKey(room))
            {
                RoomUsers[room] = new HashSet<string>();
            }

            var existingPeers = RoomUsers[room].ToList();
            RoomUsers[room].Add(Context.ConnectionId);

            await Clients.Caller.SendAsync("ExistingPeers", existingPeers);

            _logger.LogInformation("Client {ConnectionId} joined room {RoomId}", Context.ConnectionId, room);
        }

        public async Task LeaveRoom(string room)
        {
            _logger.LogInformation("Client {ConnectionId} leave room {RoomId}", Context.ConnectionId, room);
            if (_roomConnections.TryGetValue(room, out var connections))
            {
                connections.TryRemove(Context.ConnectionId, out _);
                if (connections.IsEmpty)
                {
                    _roomConnections.TryRemove(room, out _);
                    _roomManager.ScheduleRoomCleanup(room);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
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

        public async Task SendCode(string room, string code, string language)
        {
            if (await isRoomCompleted(room)) return;
            _logger.LogInformation("Code updated for room {RoomId}", room);
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(room);

            if (roomState.LanguageCodes.ContainsKey(language))
            {
                roomState.LanguageCodes[language] = code;
            }
            else
            {
                roomState.LanguageCodes.Add(language, code);
            }

            await Clients.OthersInGroup(room).SendAsync("ReceiveCode", code, language);
        }

        public async Task SendLanguage(string roomId, string language)
        {
            if (await isRoomCompleted(roomId)) return;
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            _logger.LogInformation("Language updated for room {RoomId}", roomId);
            roomState.CurrentLanguage = language;

            var codeForNewLang = roomState.LanguageCodes.ContainsKey(language)
                ? roomState.LanguageCodes[language]
                : string.Empty;

            await Clients.OthersInGroup(roomId).SendAsync("ReceiveLanguage", language, codeForNewLang);
        }

        public async Task RunCode(string roomId, string code, string language)
        {
            _logger.LogInformation("Run Code for room {RoomId}", roomId);
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);

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
                        actualJson = JsonDocument.Parse(actualOutput).RootElement;
                    }
                    catch (JsonException) { }

                    passed = false;
                    foreach (var expected in expectedOutputs)
                    {
                        if (actualJson.HasValue) 
                        {
                            try
                            {
                                var expectedJson = JsonDocument.Parse(expected).RootElement;
                                if (new JsonElementComparer().Equals(actualJson.Value, expectedJson))
                                {
                                    passed = true;
                                    break;
                                }
                            }
                            catch (JsonException) { }
                        }
                        else 
                        {
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
                }

                await Clients.Group(roomId).SendAsync("ReceiveTestResults", allTestResults);
            }
            else 
            {
                var output = await _codeExecutionService.SendRequestAsync(code, language.Equals("c++") ? "cpp" : language);
                await Clients.Group(roomId).SendAsync("ReceiveExecutionResult", output);
            }
        }

        public async Task SendProblem(string roomId, string description, string shortName, object[] testCases)
        {
            if (await isRoomCompleted(roomId)) return;
            var roomState = await _roomManager.GetOrCreateRoomStateAsync(roomId);
            roomState.ProblemDescription = description;
            roomState.ProblemShortName = shortName;
            roomState.TestCases = testCases;

            _logger.LogInformation("Problem updated for room {RoomId}", roomId);

            await Clients.OthersInGroup(roomId).SendAsync("ReceiveProblem", description, shortName, testCases);

            if (!string.IsNullOrEmpty(shortName) && testCases.Length > 0 &&
             _codeGenerationServices.TryGetValue(roomState.CurrentLanguage, out var codeGenService))
            {
                var generatedCode = codeGenService.GenerateTemplate(shortName, testCases);
                if (!string.IsNullOrEmpty(generatedCode))
                {
                    roomState.LanguageCodes[roomState.CurrentLanguage] = generatedCode;
                    await Clients.Group(roomId).SendAsync("ReceiveCode", generatedCode);
                }
            }
        }

        public async Task<bool> isRoomCompleted(string roomId)
        {
            var room = await _getCurrentRoom.ExecuteAsync(Guid.Parse(roomId));
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
