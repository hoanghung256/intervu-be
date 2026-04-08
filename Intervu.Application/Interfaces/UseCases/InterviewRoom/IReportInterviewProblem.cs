using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IReportInterviewProblem
    {
        Task<CreateRoomReportResult> ExecuteAsync(Guid interviewRoomId, CreateRoomReportRequest request, Guid userId);
    }
}
