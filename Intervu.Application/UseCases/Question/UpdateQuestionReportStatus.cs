using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class UpdateQuestionReportStatus(
        IUnitOfWork unitOfWork,
        IBackgroundService jobService) : IUpdateQuestionReportStatus
    {
        public async Task ExecuteAsync(Guid reportId, UpdateQuestionReportStatusRequest request, Guid adminUserId)
        {
            if ((request.Status == QuestionReportStatus.Resolved || request.Status == QuestionReportStatus.Dismissed)
                && string.IsNullOrWhiteSpace(request.ResolutionNote))
            {
                throw new ArgumentException("ResolutionNote is required when marking a report as Resolved or Dismissed.");
            }

            var reportRepository = unitOfWork.GetRepository<IQuestionReportRepository>();
            var questionRepository = unitOfWork.GetRepository<IQuestionRepository>();

            var report = await reportRepository.GetByIdAsync(reportId)
                ?? throw new Exception("Report not found");

            var question = await questionRepository.GetByIdAsync(report.QuestionId);
            var questionTitle = question?.Title ?? "Unknown Question";

            var now = DateTime.UtcNow;
            report.Status = request.Status;
            report.UpdatedAt = now;

            if (request.Status == QuestionReportStatus.Resolved || request.Status == QuestionReportStatus.Dismissed)
            {
                report.ResolvedBy = adminUserId;
                report.ResolvedAt = now;
                report.ResolutionNote = request.ResolutionNote?.Trim();
                report.ActionTaken = request.ActionTaken ?? ResolutionAction.NoAction;
            }

            await unitOfWork.BeginTransactionAsync();
            try
            {
                reportRepository.UpdateAsync(report);

                if (request.Status == QuestionReportStatus.Resolved
                    && request.ActionTaken == ResolutionAction.DeactivateQuestion
                    && question != null)
                {
                    question.IsHidden = true;
                    question.UpdatedAt = now;
                    questionRepository.UpdateAsync(question);
                }

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // Send notification to reporter
            jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                report.ReportedBy,
                NotificationType.SystemAnnouncement,
                "Question Report Result",
                $"Your report for question \"{(questionTitle.Length > 30 ? questionTitle.Substring(0, 30) + "..." : questionTitle)}\" has been marked as {request.Status}.",
                null,
                report.Id
            ));
        }
    }
}
