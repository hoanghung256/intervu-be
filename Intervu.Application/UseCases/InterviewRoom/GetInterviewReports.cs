using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewRoom
{
    public class GetInterviewReports(IInterviewReportRepository interviewReportRepository) : IGetInterviewReports
    {
        public async Task<PagedResult<InterviewReportItemDto>> ExecuteAsync(InterviewReportFilterRequest filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var (items, total) = await interviewReportRepository.GetPagedAsync(
                filter.Page,
                filter.PageSize,
                filter.Status,
                filter.SearchTerm,
                filter.ReporterId);

            var dtos = items.Select(x => new InterviewReportItemDto
            {
                Id = x.Id,
                InterviewRoomId = x.InterviewRoomId,
                ReporterId = x.ReportedBy,
                ReporterName = x.Reporter?.FullName ?? string.Empty,
                UserId = x.ReporterId,
                Reason = x.Reason,
                Details = x.Details,
                ExpectTo = x.ExpectTo,
                Status = x.Status,
                AdminNote = x.AdminNote,
                ResolvedAt = x.ResolvedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();

            return new PagedResult<InterviewReportItemDto>(dtos, total, filter.PageSize, filter.Page);
        }
    }
}
