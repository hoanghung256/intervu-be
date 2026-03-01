using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class AddQuestion(IUnitOfWork unitOfWork) : IAddQuestion
    {
        public async Task<Guid> ExecuteAsync(Guid experienceId, CreateQuestionRequest request, Guid userId)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = new Domain.Entities.Question
            {
                Id = Guid.NewGuid(),
                InterviewExperienceId = experienceId,
                QuestionType = request.QuestionType,
                Content = request.Content,
                Answer = request.Answer,
                CreatedAt = DateTime.UtcNow
            };

            // Auto-create first comment from the Answer if provided
            if (!string.IsNullOrWhiteSpace(request.Answer))
            {
                question.Comments.Add(new Domain.Entities.Comment
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Content = request.Answer,
                    IsAnswer = true,
                    Vote = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    CreateBy = userId,
                    UpdateBy = userId
                });
            }

            await questionRepo.AddAsync(question);
            await unitOfWork.SaveChangesAsync();

            return question.Id;
        }
    }
}
