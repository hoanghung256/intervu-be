using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class SubmitCoachEvaluation : ISubmitCoachEvaluation
    {
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly IBackgroundService _jobService;
        private readonly ILogger<SubmitCoachEvaluation> _logger;

        public SubmitCoachEvaluation(IInterviewRoomRepository roomRepo, IBackgroundService jobService, ILogger<SubmitCoachEvaluation> logger)
        {
            _roomRepo = roomRepo;
            _jobService = jobService;
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
                _jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                    room.CandidateId.Value,
                    NotificationType.FeedbackReceived,
                    "Evaluation Completed",
                    "Your coach has submitted their evaluation. Check your dashboard for details.",
                    "/interview?tab=past",
                    null
                ));
            }

            // TODO: Send email notification to both parties with evaluation summary

            _logger.LogInformation("Coach {CoachId} submitted evaluation for interview room {RoomId}", coachId, interviewRoomId);
        }
    }
}
