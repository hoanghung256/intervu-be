using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class RejectGeneratedQuestion(IUnitOfWork unitOfWork) : IRejectGeneratedQuestion
    {
        public async Task ExecuteAsync(Guid generatedQuestionId, Guid reviewerUserId)
        {
            var generatedRepo = unitOfWork.GetRepository<IGeneratedQuestionRepository>();
            var generated = await generatedRepo.GetByIdAsync(generatedQuestionId)
                ?? throw new Exception("Generated question not found");

            if (generated.Status != GeneratedQuestionStatus.PendingReview)
            {
                throw new Exception("Generated question is already reviewed");
            }

            generated.Status = GeneratedQuestionStatus.Rejected;
            generatedRepo.UpdateAsync(generated);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
