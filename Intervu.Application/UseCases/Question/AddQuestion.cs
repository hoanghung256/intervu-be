using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Notification;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class AddQuestion(
        IUnitOfWork unitOfWork,
        IBackgroundService jobService) : IAddQuestion
    {
        public async Task<AddQuestionResult> ExecuteAsync(Guid experienceId, CreateQuestionRequest request, Guid userId)
        {
            // Link to existing question: post the answer as a Comment
            if (request.LinkedQuestionId.HasValue)
            {
                var commentRepo = unitOfWork.GetRepository<ICommentRepository>();
                var now = DateTime.UtcNow;
                await commentRepo.AddAsync(new Domain.Entities.Comment
                {
                    Id = Guid.NewGuid(),
                    QuestionId = request.LinkedQuestionId.Value,
                    Content = request.Answer ?? request.Content ?? string.Empty,
                    IsAnswer = true,
                    Vote = 0,
                    CreatedAt = now,
                    UpdateAt = now,
                    CreateBy = userId,
                    UpdateBy = userId
                });
                await unitOfWork.SaveChangesAsync();

                return new AddQuestionResult
                {
                    QuestionId = request.LinkedQuestionId.Value,
                    IsLinked = true
                };
            }

            // Normal path: create a new question
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = new Domain.Entities.Question
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                InterviewExperienceId = experienceId,
                Level = request.Level,
                Round = request.Round,
                Category = request.Category,
                Status = Domain.Entities.Constants.QuestionConstants.QuestionStatus.Pending,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var cid in request.CompanyIds)
                question.QuestionCompanies.Add(new QuestionCompany { QuestionId = question.Id, CompanyId = cid });

            foreach (var r in request.Roles)
                question.QuestionRoles.Add(new QuestionRole { QuestionId = question.Id, Role = r });

            foreach (var tid in request.TagIds)
                question.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tid });

            // Optional initial answer stored as a comment
            if (!string.IsNullOrWhiteSpace(request.Answer))
            {
                var now = DateTime.UtcNow;
                question.Comments.Add(new Domain.Entities.Comment
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Content = request.Answer,
                    IsAnswer = true,
                    Vote = 0,
                    CreatedAt = now,
                    UpdateAt = now,
                    CreateBy = userId,
                    UpdateBy = userId
                });
            }

            await questionRepo.AddAsync(question);
            await unitOfWork.SaveChangesAsync();

            var titlePreview = question.Title.Length > 40
                ? question.Title.Substring(0, 40) + "..."
                : question.Title;

            jobService.Enqueue<INotificationUseCase>(uc => uc.BroadcastToRoleAsync(
                "Admin",
                NotificationType.SystemAnnouncement,
                "New question awaiting moderation",
                $"A new question \"{titlePreview}\" has been contributed and is pending review.",
                "/admin/question-bank"
            ));

            return new AddQuestionResult
            {
                QuestionId = question.Id,
                IsLinked = false
            };
        }
    }
}

