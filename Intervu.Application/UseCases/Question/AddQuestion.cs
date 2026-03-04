using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class AddQuestion(IUnitOfWork unitOfWork) : IAddQuestion
    {
        public async Task<AddQuestionResult> ExecuteAsync(Guid experienceId, CreateQuestionRequest request, Guid userId)
        {
            // --- Link to existing question: post an answer ---
            if (request.LinkedQuestionId.HasValue)
            {
                var answerRepo = unitOfWork.GetRepository<IAnswerRepository>();
                var answer = new Answer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = request.LinkedQuestionId.Value,
                    AuthorId = userId,
                    Content = request.Answer ?? request.Content,
                    Upvotes = 0,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await answerRepo.AddAsync(answer);
                await unitOfWork.SaveChangesAsync();

                return new AddQuestionResult
                {
                    QuestionId = request.LinkedQuestionId.Value,
                    IsLinked = true
                };
            }

            // --- Normal path: create a new question ---
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
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // M:M – companies
            foreach (var cid in request.CompanyIds)
                question.QuestionCompanies.Add(new QuestionCompany { QuestionId = question.Id, CompanyId = cid });

            // M:M – roles
            foreach (var r in request.Roles)
                question.QuestionRoles.Add(new QuestionRole { QuestionId = question.Id, Role = r });

            // M:M – tags
            foreach (var tid in request.TagIds)
                question.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tid });

            // Optional initial answer
            if (!string.IsNullOrWhiteSpace(request.Answer))
            {
                question.Answers.Add(new Answer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    AuthorId = userId,
                    Content = request.Answer,
                    Upvotes = 0,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await questionRepo.AddAsync(question);
            await unitOfWork.SaveChangesAsync();

            return new AddQuestionResult
            {
                QuestionId = question.Id,
                IsLinked = false
            };
        }
    }
}
