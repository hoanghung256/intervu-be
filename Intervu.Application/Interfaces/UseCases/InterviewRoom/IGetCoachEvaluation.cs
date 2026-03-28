using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetCoachEvaluation
    {
        Task<CoachEvaluationResponseDto> ExecuteAsync(Guid interviewRoomId, Guid userId);
    }
}
