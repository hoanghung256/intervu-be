using Intervu.Application.DTOs.Question;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class ReportQuestion(
        IUnitOfWork unitOfWork,
        IBackgroundService jobService) : IReportQuestion
    {
        private const int MaxReasonLength = 1000;

        public async Task<ReportQuestionResult> ExecuteAsync(Guid questionId, ReportQuestionRequest request, Guid userId)
        {
            var reason = request?.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BadRequestException("Reason is required");
            }

            if (reason.Length > MaxReasonLength)
            {
                throw new BadRequestException($"Reason must be less than or equal to {MaxReasonLength} characters");
            }

            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var reportRepo = unitOfWork.GetRepository<IQuestionReportRepository>();

            var question = await questionRepo.GetByIdAsync(questionId);
            if (question == null)
            {
                throw new NotFoundException("Question not found");
            }

            var hasPendingReport = await reportRepo.HasPendingReportAsync(questionId, userId);
            if (hasPendingReport)
            {
                throw new BadRequestException("You already reported this question");
            }

            var now = DateTime.UtcNow;
            var report = new QuestionReport
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                ReportedBy = userId,
                Reason = reason,
                Status = QuestionReportStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };

            await reportRepo.AddAsync(report);
            await unitOfWork.SaveChangesAsync();

            jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                userId,
                NotificationType.SystemAnnouncement,
                "Question Report Submitted",
                $"Your report for question \"{(question.Title.Length > 30 ? question.Title.Substring(0, 30) + "..." : question.Title)}\" has been submitted successfully and is being reviewed.",
                "/history",
                null
            ));

            // Notify all admins via system event
            jobService.Enqueue<INotificationUseCase>(uc => uc.BroadcastToRoleAsync(
                "Admin",
                NotificationType.SystemAnnouncement,
                "New Question Report",
                $"A new report has been submitted for question: {question.Title}",
                "/admin/reports"
            ));

            return new ReportQuestionResult
            {
                ReportId = report.Id
            };
        }
    }
}