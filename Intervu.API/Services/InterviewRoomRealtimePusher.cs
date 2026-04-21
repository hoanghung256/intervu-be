using Intervu.API.Hubs;
using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.API.Services
{
    /// <summary>
    /// SignalR-backed implementation of <see cref="IInterviewRoomRealtimePusher"/>.
    /// Sits alongside <see cref="SignalRNotificationPusher"/> but targets
    /// InterviewRoomHub groups (keyed by roomId) rather than per-user groups.
    ///
    /// For "Send to Editor" we intentionally mirror the existing
    /// <c>InterviewRoomHub.SendProblem</c> flow — broadcast the problem,
    /// sync the in-memory <see cref="RoomState"/>, regenerate the starter
    /// code template for the current language, and broadcast
    /// <c>ReceiveCode</c> so the candidate's Monaco editor actually picks up
    /// the new function-name / test-case signature. Without the template
    /// regeneration the editor keeps the old code and appears "unchanged".
    /// </summary>
    public class InterviewRoomRealtimePusher : IInterviewRoomRealtimePusher
    {
        private readonly IHubContext<InterviewRoomHub> _hubContext;
        private readonly RoomManagerService _roomManager;
        private readonly IReadOnlyDictionary<string, ICodeGenerationService> _codeGenerationServices;
        private readonly ILogger<InterviewRoomRealtimePusher> _logger;

        public InterviewRoomRealtimePusher(
            IHubContext<InterviewRoomHub> hubContext,
            RoomManagerService roomManager,
            IEnumerable<ICodeGenerationService> codeGenerationServices,
            ILogger<InterviewRoomRealtimePusher> logger)
        {
            _hubContext = hubContext;
            _roomManager = roomManager;
            _codeGenerationServices = codeGenerationServices
                .ToDictionary(s => s.Language, StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        public async Task PushPreparedQuestionStatusChangedAsync(Guid interviewRoomId, PreparedQuestionDto dto)
        {
            await _hubContext.Clients.Group(interviewRoomId.ToString())
                .SendAsync("PreparedQuestionStatusChanged", dto);
        }

        public async Task PushProblemToRoomAsync(
            Guid interviewRoomId,
            Guid? excludeUserId,
            string description,
            string shortName,
            object[] testCases)
        {
            var roomIdStr = interviewRoomId.ToString();
            var groupClients = _hubContext.Clients.Group(roomIdStr);

            // 1) Same payload shape as InterviewRoomHub.SendProblem -> "ReceiveProblem",
            //    so the existing candidate frontend handler works unchanged.
            await groupClients.SendAsync("ReceiveProblem", description, shortName, testCases);

            // 2) Keep the in-memory RoomState in sync with what we just broadcast.
            //    Late-joiners get this via ReceiveFullState on connect.
            RoomState? roomState = null;
            try
            {
                roomState = await _roomManager.GetOrCreateRoomStateAsync(roomIdStr);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "RoomManager lookup failed for {RoomId} during Send-to-Editor broadcast",
                    roomIdStr);
            }

            if (roomState == null)
            {
                return;
            }

            roomState.ProblemDescription = description ?? string.Empty;
            roomState.ProblemShortName = shortName ?? string.Empty;
            roomState.TestCases = testCases ?? Array.Empty<object>();

            // 3) Regenerate the Monaco starter-code template for the current language.
            //    This is the same branch InterviewRoomHub.SendProblem runs — without
            //    it, the editor keeps the previous code and the "no change" symptom
            //    appears even though ReceiveProblem arrived correctly.
            if (string.IsNullOrEmpty(shortName)
                || testCases == null
                || testCases.Length == 0)
            {
                return;
            }

            var language = roomState.CurrentLanguage;
            if (string.IsNullOrEmpty(language)
                || !_codeGenerationServices.TryGetValue(language, out var codeGenService))
            {
                return;
            }

            try
            {
                var generatedCode = codeGenService.GenerateTemplate(shortName, testCases);
                if (string.IsNullOrEmpty(generatedCode))
                {
                    return;
                }

                roomState.LanguageCodes[language] = generatedCode;
                await groupClients.SendAsync("ReceiveCode", generatedCode, language);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Starter-code generation failed for room {RoomId}, language {Language}",
                    roomIdStr, language);
            }
        }
    }
}
