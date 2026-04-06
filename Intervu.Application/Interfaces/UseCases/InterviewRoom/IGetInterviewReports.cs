using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.Application.Interfaces.UseCases.InterviewRoom
{
    public interface IGetInterviewReports
    {
        Task<PagedResult<InterviewReportItemDto>> ExecuteAsync(InterviewReportFilterRequest filter);
    }
}
