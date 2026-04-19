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
    public class ModerateQuestion(
        IUnitOfWork unitOfWork,
        IBackgroundService jobService) : IModerateQuestion
    {
        public async Task ExecuteAsync(Guid questionId, QuestionStatus newStatus, Guid adminUserId)
        {
            if (newStatus != QuestionStatus.Approved && newStatus != QuestionStatus.Rejected)
            {
                throw new ArgumentException("Moderation can only transition to Approved or Rejected.");
            }

            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = await questionRepo.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");

            if (question.Status != QuestionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending questions can be moderated.");
            }

            question.Status = newStatus;
            question.UpdatedAt = DateTime.UtcNow;
            questionRepo.UpdateAsync(question);
            await unitOfWork.SaveChangesAsync();

            if (question.CreatedBy.HasValue)
            {
                var titlePreview = question.Title.Length > 30
                    ? question.Title.Substring(0, 30) + "..."
                    : question.Title;

                if (newStatus == QuestionStatus.Approved)
                {
                    jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                        question.CreatedBy.Value,
                        NotificationType.SystemAnnouncement,
                        "Your question was approved",
                        $"Your contributed question \"{titlePreview}\" has been approved and is now live in the question bank.",
                        $"/questions/{question.Id}",
                        question.Id
                    ));
                }
                else
                {
                    jobService.Enqueue<INotificationUseCase>(uc => uc.CreateAsync(
                        question.CreatedBy.Value,
                        NotificationType.SystemAnnouncement,
                        "Your question was rejected",
                        $"Your contributed question \"{titlePreview}\" was not approved by moderators.",
                        null,
                        question.Id
                    ));
                }
            }
        }
    }
}
