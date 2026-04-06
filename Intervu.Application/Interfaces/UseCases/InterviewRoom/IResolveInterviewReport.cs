using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IResolveInterviewReport
    {
        Task ExecuteAsync(ResolveRoomReportRequest request, Guid adminId);
    }
}
