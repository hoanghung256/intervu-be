using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IUpdateQuestionReportStatus
    {
        Task ExecuteAsync(Guid reportId, QuestionReportStatus newStatus);
    }
}
