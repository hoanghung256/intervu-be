using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.PreparedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants.PreparedQuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.PreparedQuestion
{
    public class UpdatePreparedQuestion(IUnitOfWork unitOfWork) : IUpdatePreparedQuestion
    {
        public async Task<PreparedQuestionDto> ExecuteAsync(
            Guid preparedQuestionId,
            UpdatePreparedQuestionRequest request,
            Guid userId)
        {
            if (request == null)
            {
                throw new BadRequestException("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Title is required");
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new BadRequestException("Description is required");
            }

            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();
            var entity = await preparedRepo.GetByIdAsync(preparedQuestionId)
                ?? throw new NotFoundException("Prepared question not found");

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, entity.InterviewRoomId, userId);

            entity.Title = request.Title.Trim();
            entity.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.DisplayCategoryLabel))
            {
                entity.DisplayCategoryLabel = request.DisplayCategoryLabel.Trim();
            }

            if (entity.InteractionType == PreparedQuestionInteractionType.Coding)
            {
                entity.FunctionName = string.IsNullOrWhiteSpace(request.FunctionName)
                    ? null
                    : request.FunctionName.Trim();
                entity.TestCases = PreparedQuestionMapper.NormalizeTestCasesForPersistence(request.TestCases);
            }
            else
            {
                // Keep non-coding rows clean of coding-only fields.
                entity.FunctionName = null;
                entity.TestCases = null;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            preparedRepo.UpdateAsync(entity);
            await unitOfWork.SaveChangesAsync();

            return PreparedQuestionMapper.ToDto(entity);
        }
    }
}
