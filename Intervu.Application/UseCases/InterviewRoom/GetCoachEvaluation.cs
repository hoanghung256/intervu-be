using System.Text.Json;
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

        public async Task<CoachEvaluationResponseDto> ExecuteAsync(Guid interviewRoomId, Guid userId)
        {
            var room = await _roomRepo.GetByIdWithDetailsAsync(interviewRoomId)
                ?? throw new NotFoundException("Interview room not found");

            if (room.CoachId != userId && room.CandidateId != userId)
            {
                throw new ForbiddenException("You are not authorized to view this evaluation");
            }

            if (room.Status != InterviewRoomStatus.Ongoing && room.Status != InterviewRoomStatus.Completed)
            {
                throw new ConflictException("Evaluation is only available while the interview is ongoing or after it is completed");
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

            var (others, hireDecision) = ParseMetadata(room.EvaluationResultsJson);

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
                }).ToList(),
                Others = others,
                HireDecision = hireDecision,
                EvaluationStructureJson = room.EvaluationResultsJson
            };
        }

        private static (string? others, string? hireDecision) ParseMetadata(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return (null, null);
            }

            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return (null, null);
                }

                var others = TryGetString(root, "others");
                var hireDecision = TryGetString(root, "hireDecision") ?? TryGetString(root, "hideDecision");
                return (others, hireDecision);
            }
            catch
            {
                return (null, null);
            }
        }

        private static string? TryGetString(JsonElement root, string propertyName)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }

                    if (property.Value.ValueKind == JsonValueKind.True)
                    {
                        return "yes";
                    }

                    if (property.Value.ValueKind == JsonValueKind.False)
                    {
                        return "no";
                    }
                }
            }

            return null;
        }
    }
}
