using Intervu.Application.DTOs.Question;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class ReportQuestion(IUnitOfWork unitOfWork) : IReportQuestion
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
                CreatedAt = now,
                UpdatedAt = now
            };

            await reportRepo.AddAsync(report);
            await unitOfWork.SaveChangesAsync();

            return new ReportQuestionResult
            {
                ReportId = report.Id
            };
        }
    }
}