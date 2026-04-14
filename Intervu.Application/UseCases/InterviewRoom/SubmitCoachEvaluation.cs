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


        public async Task ExecuteAsync(Guid interviewRoomId, Guid coachId, List<EvaluationResultDto> results)
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

            if (results == null || results.Count == 0)
            {
                throw new BadRequestException("Evaluation results are required");
            }

            if (results.Any(r => r.Score < 0 || r.Score > 10))
            {
                throw new BadRequestException("Scores must be between 0 and 10");
            }

            room.EvaluationResults = results.Select(r => new EvaluationResult
            {
                Type = r.Type,
                Question = r.Question,
                Answer = r.Answer,
                Score = r.Score
            }).ToList();
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

                // Trigger roadmap progress update for the candidate in the background
                var coachFullName = coach?.FullName ?? string.Empty;
                var candidateId = room.CandidateId.Value;
                var roomId = interviewRoomId;
                _jobService.Enqueue<IAssessmentService>(svc =>
                    svc.UpdateRoadmapAfterInterviewAsync(candidateId, roomId, coachFullName));
            }

            _logger.LogInformation("Coach {CoachId} submitted evaluation for interview room {RoomId}", coachId, interviewRoomId);
        }
    }
}
