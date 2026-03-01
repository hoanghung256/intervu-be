using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class UpdateQuestion(IUnitOfWork unitOfWork) : IUpdateQuestion
    {
        public async Task ExecuteAsync(Guid questionId, UpdateQuestionRequest request, Guid userId)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = await questionRepo.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");

            question.QuestionType = request.QuestionType;
            question.Content = request.Content;
            question.Answer = request.Answer;

            questionRepo.UpdateAsync(question);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
