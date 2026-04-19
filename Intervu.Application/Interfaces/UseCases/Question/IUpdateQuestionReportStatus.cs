using Intervu.Application.DTOs.Question;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IUpdateQuestionReportStatus
    {
        Task ExecuteAsync(Guid reportId, UpdateQuestionReportStatusRequest request, Guid adminUserId);
    }
}
