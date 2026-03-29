using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface ISaveCoachEvaluationDraft
    {
        Task ExecuteAsync(Guid interviewRoomId, Guid coachId, List<EvaluationResultDto> results);
    }
}
