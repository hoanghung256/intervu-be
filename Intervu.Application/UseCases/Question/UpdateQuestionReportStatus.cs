using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class UpdateQuestionReportStatus(IQuestionReportRepository reportRepository) : IUpdateQuestionReportStatus
    {
        public async Task ExecuteAsync(Guid reportId, QuestionReportStatus newStatus)
        {
            var report = await reportRepository.GetByIdAsync(reportId)
                ?? throw new Exception("Report not found");

            report.Status = newStatus;
            report.UpdatedAt = DateTime.UtcNow;

            reportRepository.UpdateAsync(report);
            await reportRepository.SaveChangesAsync();
        }
    }
}
