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
    public class AddCustomPreparedQuestion(IUnitOfWork unitOfWork) : IAddCustomPreparedQuestion
    {
        public async Task<PreparedQuestionDto> ExecuteAsync(
            Guid interviewRoomId,
            CreateCustomPreparedQuestionRequest request,
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

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, interviewRoomId, userId);

            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();
            var nextSortOrder = await preparedRepo.GetMaxSortOrderAsync(interviewRoomId) + 1;

            var now = DateTime.UtcNow;
            var entity = new Domain.Entities.PreparedQuestion
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = interviewRoomId,
                CreatedBy = userId,
                SourceBankQuestionId = null,
                InteractionType = request.InteractionType,
                DisplayCategoryLabel = NormalizeLabel(request.DisplayCategoryLabel, request.InteractionType),
                Title = request.Title.Trim(),
                Description = request.Description,
                FunctionName = request.InteractionType == PreparedQuestionInteractionType.Coding
                    ? request.FunctionName?.Trim()
                    : null,
                TestCases = request.InteractionType == PreparedQuestionInteractionType.Coding
                    ? PreparedQuestionMapper.NormalizeTestCasesForPersistence(request.TestCases)
                    : null,
                Status = PreparedQuestionStatus.Pending,
                AskedAt = null,
                SortOrder = nextSortOrder,
                CreatedAt = now,
                UpdatedAt = now
            };

            await preparedRepo.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();

            return PreparedQuestionMapper.ToDto(entity);
        }

        private static string NormalizeLabel(string? incoming, PreparedQuestionInteractionType interactionType)
        {
            if (!string.IsNullOrWhiteSpace(incoming))
            {
                return incoming.Trim();
            }

            return interactionType == PreparedQuestionInteractionType.Coding ? "Coding" : "Behavioral";
        }
    }
}
