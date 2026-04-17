using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface ISubmitCoachEvaluation
    {
        Task ExecuteAsync(Guid interviewRoomId, Guid coachId, SubmitCoachEvaluationRequest request);
    }
}
