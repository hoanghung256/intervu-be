using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class GetCoachEvaluation : IGetCoachEvaluation
    {
        private readonly IInterviewRoomRepository _roomRepo;

        public GetCoachEvaluation(IInterviewRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<CoachEvaluationResponseDto> ExecuteAsync(Guid interviewRoomId, Guid coachId)
        {
            var room = await _roomRepo.GetByIdWithDetailsAsync(interviewRoomId)
                ?? throw new NotFoundException("Interview room not found");

            if (room.CoachId != coachId)
            {
                throw new ForbiddenException("You are not the coach for this interview");
            }

            if (room.Status != InterviewRoomStatus.Completed)
            {
                throw new ConflictException("Evaluation is only available after the interview is completed");
            }

            var results = room.EvaluationResults ?? new List<EvaluationResult>();
            if ((results == null || results.Count == 0) && room.CoachInterviewService?.InterviewType?.EvaluationStructure != null)
            {
                results = room.CoachInterviewService.InterviewType.EvaluationStructure
                    .Select(item => new EvaluationResult
                    {
                        Type = item.Type,
                        Question = item.Question,
                        Answer = string.Empty,
                        Score = 0
                    })
                    .ToList();
            }

            return new CoachEvaluationResponseDto
            {
                InterviewRoomId = room.Id,
                CoachId = room.CoachId,
                CandidateId = room.CandidateId,
                ScheduledTime = room.ScheduledTime,
                Status = room.Status,
                IsEvaluationCompleted = room.IsEvaluationCompleted,
                EvaluationResults = results.Select(r => new EvaluationResultDto
                {
                    Type = r.Type,
                    Question = r.Question,
                    Answer = r.Answer,
                    Score = r.Score
                }).ToList()
            };
        }
    }
}
