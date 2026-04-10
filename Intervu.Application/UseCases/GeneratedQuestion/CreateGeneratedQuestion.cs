using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.GeneratedQuestion
{
    public class CreateGeneratedQuestion(IUnitOfWork unitOfWork) : ICreateGeneratedQuestion
    {
        public async Task<Guid> ExecuteAsync(CreateGeneratedQuestionRequest request, Guid creatorUserId)
        {
            var generatedRepo = unitOfWork.GetRepository<IGeneratedQuestionRepository>();

            var item = new Domain.Entities.GeneratedQuestion
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = request.InterviewRoomId,
                Title = request.Title?.Trim() ?? string.Empty,
                Content = request.Content.Trim(),
                Status = GeneratedQuestionStatus.PendingReview
            };

            await generatedRepo.AddAsync(item);
            await unitOfWork.SaveChangesAsync();

            return item.Id;
        }
    }
}
