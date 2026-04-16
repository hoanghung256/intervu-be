using System.Text.Json;
using System.Threading;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class SubmitCoachEvaluation : ISubmitCoachEvaluation
    {
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly IBackgroundService _jobService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SubmitCoachEvaluation> _logger;

        public SubmitCoachEvaluation(
            IInterviewRoomRepository roomRepo,
            IBackgroundService jobService,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<SubmitCoachEvaluation> logger)
        {
            _roomRepo = roomRepo;
            _jobService = jobService;
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid interviewRoomId, Guid coachId, SubmitCoachEvaluationRequest request)
        {
            var room = await _roomRepo.GetByIdWithDetailsAsync(interviewRoomId)
                ?? throw new NotFoundException("Interview room not found");

            if (room.CoachId != coachId)
            {
                throw new ForbiddenException("You are not the coach for this interview");
            }

            if (room.Status != InterviewRoomStatus.Ongoing && room.Status != InterviewRoomStatus.Completed)
            {
                throw new ConflictException("Evaluation is only allowed while the interview is ongoing or after it is completed");
            }

            var merged = BuildPayload(request ?? new SubmitCoachEvaluationRequest());

            if (merged.Results == null || merged.Results.Count == 0)
            {
                throw new BadRequestException("Evaluation results are required");
            }

            if (merged.Results.Any(r => r.Score < 0 || r.Score > 10))
            {
                throw new BadRequestException("Scores must be between 0 and 10");
            }

            room.EvaluationResultsJson = JsonSerializer.Serialize(merged);
            room.IsEvaluationCompleted = true;

            _roomRepo.UpdateAsync(room);
            await _roomRepo.SaveChangesAsync();

            if (room.CandidateId.HasValue)
            {
                var candidate = await _userRepository.GetByIdAsync(room.CandidateId.Value);
                var coach = room.CoachId.HasValue
                    ? await _userRepository.GetByIdAsync(room.CoachId.Value)
                    : null;

                _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                    room.CandidateId.Value,
                    NotificationType.FeedbackReceived,
                    "Evaluation Completed",
                    "Your coach has submitted their evaluation. Check your dashboard for details.",
                    "/interview?tab=past",
                    null
                ));

                if (candidate != null)
                {
                    try
                    {
                        var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
                        var placeholders = new Dictionary<string, string>
                        {
                            ["CandidateName"] = candidate.FullName,
                            ["CoachName"] = coach?.FullName ?? "Coach",
                            ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/interview?tab=past"
                        };

                        _jobService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                            candidate.Email,
                            "EvaluationReady",
                            placeholders));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to enqueue EvaluationReady email for room {RoomId}", interviewRoomId);
                    }
                }

                var coachFullName = coach?.FullName ?? string.Empty;
                var candidateId = room.CandidateId.Value;
                var roomId = interviewRoomId;
                _jobService.Enqueue<IAssessmentService>(svc =>
                    svc.UpdateRoadmapAfterInterviewAsync(candidateId, roomId, coachFullName, CancellationToken.None));
            }

            _logger.LogInformation("Coach {CoachId} submitted evaluation for interview room {RoomId}", coachId, interviewRoomId);
        }

        private static SubmitEvaluationStructurePayload BuildPayload(SubmitCoachEvaluationRequest request)
        {
            var payload = new SubmitEvaluationStructurePayload();

            var rawJson = string.IsNullOrWhiteSpace(request.EvaluationStructureJson)
                ? request.EvaluationStructure
                : request.EvaluationStructureJson;

            if (!string.IsNullOrWhiteSpace(rawJson))
            {
                TryMergeFromEvaluationStructureJson(payload, rawJson!);
            }

            if (request.Results != null && request.Results.Count > 0)
            {
                payload.Results = request.Results.Select(ToDomainResult).ToList();
            }

            payload.HireDecision = NormalizeHireDecision(payload.HireDecision);
            return payload;
        }

        private static void TryMergeFromEvaluationStructureJson(SubmitEvaluationStructurePayload payload, string rawJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                if (TryGetString(root, "others", out var others))
                {
                    payload.Others = others;
                }

                if (TryGetString(root, "hireDecision", out var hireDecision) || TryGetString(root, "hideDecision", out hireDecision))
                {
                    payload.HireDecision = hireDecision;
                }

                if (TryGetProperty(root, "results", out var resultsElement) || TryGetProperty(root, "evaluationResults", out resultsElement))
                {
                    var parsed = ParseResults(resultsElement);
                    if (parsed.Count > 0)
                    {
                        payload.Results = parsed;
                    }
                }
            }
            catch
            {
                // ignore malformed JSON from client and fallback to request.Results
            }
        }

        private static bool TryGetString(JsonElement root, string propertyName, out string? value)
        {
            value = null;
            if (!TryGetProperty(root, propertyName, out var element))
            {
                return false;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                value = element.GetString();
                return true;
            }

            if (element.ValueKind == JsonValueKind.True)
            {
                value = "yes";
                return true;
            }

            if (element.ValueKind == JsonValueKind.False)
            {
                value = "no";
                return true;
            }

            return false;
        }

        private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement value)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static List<EvaluationResult> ParseResults(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array)
            {
                return new List<EvaluationResult>();
            }

            try
            {
                var dtos = JsonSerializer.Deserialize<List<EvaluationResultDto>>(element.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<EvaluationResultDto>();

                return dtos.Select(ToDomainResult).ToList();
            }
            catch
            {
                return new List<EvaluationResult>();
            }
        }

        private static EvaluationResult ToDomainResult(EvaluationResultDto dto)
        {
            return new EvaluationResult
            {
                Type = dto.Type,
                Question = dto.Question,
                Answer = dto.Answer,
                Score = dto.Score
            };
        }

        private static string? NormalizeHireDecision(string? hireDecision)
        {
            if (string.IsNullOrWhiteSpace(hireDecision))
            {
                return null;
            }

            var normalized = hireDecision.Trim().ToLowerInvariant();
            if (normalized == "yes" || normalized == "true")
            {
                return "yes";
            }

            if (normalized == "no" || normalized == "false")
            {
                return "no";
            }

            return hireDecision;
        }

        private sealed class SubmitEvaluationStructurePayload
        {
            public List<EvaluationResult> Results { get; set; } = new();
            public string? Others { get; set; }
            public string? HireDecision { get; set; }
        }
    }
}
