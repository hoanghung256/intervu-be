using Intervu.Application.DTOs.Question;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface IReportQuestion
    {
        Task<ReportQuestionResult> ExecuteAsync(Guid questionId, ReportQuestionRequest request, Guid userId);
    }
}