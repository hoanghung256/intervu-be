using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class UpdateQuestionReportStatus(
        IQuestionReportRepository reportRepository,
        IQuestionRepository questionRepository,
        IBackgroundService jobService) : IUpdateQuestionReportStatus
    {
        public async Task ExecuteAsync(Guid reportId, QuestionReportStatus newStatus)
        {
            var report = await reportRepository.GetByIdAsync(reportId)
                ?? throw new Exception("Report not found");

            var question = await questionRepository.GetByIdAsync(report.QuestionId);
            var questionTitle = question?.Title ?? "Unknown Question";

            report.Status = newStatus;
            report.UpdatedAt = DateTime.UtcNow;

            reportRepository.UpdateAsync(report);
            await reportRepository.SaveChangesAsync();

            // Send notification to reporter
            jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                report.ReportedBy,
                NotificationType.SystemAnnouncement,
                "Question Report Result",
                $"Your report for question \"{(questionTitle.Length > 30 ? questionTitle.Substring(0, 30) + "..." : questionTitle)}\" has been marked as {newStatus}.",
                "/history",
                null
            ));
        }
    }
}
