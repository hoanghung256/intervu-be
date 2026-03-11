using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class DeleteQuestion(IUnitOfWork unitOfWork) : IDeleteQuestion
    {
        public async Task ExecuteAsync(Guid questionId, Guid userId)
        {
            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var question = await questionRepo.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");

            questionRepo.DeleteAsync(question);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
