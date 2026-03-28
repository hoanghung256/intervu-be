using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetQuestionReports(IQuestionReportRepository reportRepository) : IGetQuestionReports
    {
        public async Task<PagedResult<QuestionReportItemDto>> ExecuteAsync(QuestionReportFilterRequest filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var (items, total) = await reportRepository.GetPagedAsync(
                filter.Page,
                filter.PageSize,
                filter.Status,
                filter.SearchTerm);

            var dtos = items.Select(r => new QuestionReportItemDto
            {
                Id = r.Id,
                QuestionId = r.QuestionId,
                QuestionTitle = r.Question?.Title ?? string.Empty,
                ReporterId = r.ReportedBy,
                ReporterName = r.Reporter?.FullName ?? string.Empty,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            return new PagedResult<QuestionReportItemDto>(dtos, total, filter.PageSize, filter.Page);
        }
    }
}
