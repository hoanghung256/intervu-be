using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IGetQuestionReports
    {
        Task<PagedResult<QuestionReportItemDto>> ExecuteAsync(QuestionReportFilterRequest filter);
    }
}
