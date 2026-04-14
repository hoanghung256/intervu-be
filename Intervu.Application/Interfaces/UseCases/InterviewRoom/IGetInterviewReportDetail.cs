using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetInterviewReportDetail
    {
        Task<InterviewRoomReportDetailDto> ExecuteAsync(Guid interviewRoomId);
    }
}
