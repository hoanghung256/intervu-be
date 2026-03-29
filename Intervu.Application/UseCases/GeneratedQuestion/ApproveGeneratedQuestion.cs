using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class ApproveGeneratedQuestion(IUnitOfWork unitOfWork) : IApproveGeneratedQuestion
    {
        public async Task<Guid> ExecuteAsync(Guid generatedQuestionId, ApproveGeneratedQuestionRequest request, Guid reviewerUserId)
        {
            var generatedRepo = unitOfWork.GetRepository<IGeneratedQuestionRepository>();
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();

            var generated = await generatedRepo.GetByIdAsync(generatedQuestionId)
                ?? throw new Exception("Generated question not found");

            if (generated.Status != GeneratedQuestionStatus.PendingReview)
            {
                throw new Exception("Generated question is already reviewed");
            }

            var now = DateTime.UtcNow;
            var questionId = Guid.NewGuid();
            var question = new Domain.Entities.Question
            {
                Id = questionId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? generated.Title : request.Title.Trim(),
                Content = string.IsNullOrWhiteSpace(request.Content) ? generated.Content : request.Content.Trim(),
                InterviewExperienceId = request.InterviewExperienceId,
                Level = request.Level,
                Round = request.Round,
                Category = request.Category,
                CreatedBy = reviewerUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var cid in request.CompanyIds)
                question.QuestionCompanies.Add(new QuestionCompany { QuestionId = question.Id, CompanyId = cid });

            foreach (var role in request.Roles)
                question.QuestionRoles.Add(new QuestionRole { QuestionId = question.Id, Role = role });

            foreach (var tagId in request.TagIds ?? Enumerable.Empty<Guid>())
                question.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tagId });

            await questionRepo.AddAsync(question);

            generated.Title = question.Title;
            generated.Content = question.Content;
            generated.Status = GeneratedQuestionStatus.Approved;
            generatedRepo.UpdateAsync(generated);

            await unitOfWork.SaveChangesAsync();

            return questionId;
        }
    }
}
