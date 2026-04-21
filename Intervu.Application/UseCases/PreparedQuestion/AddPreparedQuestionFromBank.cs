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
    public class AddPreparedQuestionFromBank(IUnitOfWork unitOfWork) : IAddPreparedQuestionFromBank
    {
        public async Task<PreparedQuestionDto> ExecuteAsync(
            Guid interviewRoomId,
            ImportBankQuestionRequest request,
            Guid userId)
        {
            if (request == null || request.BankQuestionId == Guid.Empty)
            {
                throw new BadRequestException("bankQuestionId is required");
            }

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, interviewRoomId, userId);

            var questionRepo = unitOfWork.GetRepository<IQuestionRepository>();
            var bankQuestion = await questionRepo.GetByIdAsync(request.BankQuestionId)
                ?? throw new NotFoundException("Bank question not found");

            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();

            var existing = await preparedRepo.FindByRoomAndBankQuestionAsync(interviewRoomId, bankQuestion.Id);
            if (existing != null)
            {
                throw new ConflictException("This bank question has already been added to the roadmap");
            }

            var nextSortOrder = await preparedRepo.GetMaxSortOrderAsync(interviewRoomId) + 1;
            var interactionType = PreparedQuestionMapper.MapBankToInteractionType(
                bankQuestion.Category, bankQuestion.Round);

            var now = DateTime.UtcNow;
            var entity = new Domain.Entities.PreparedQuestion
            {
                Id = Guid.NewGuid(),
                InterviewRoomId = interviewRoomId,
                CreatedBy = userId,
                SourceBankQuestionId = bankQuestion.Id,
                InteractionType = interactionType,
                DisplayCategoryLabel = PreparedQuestionMapper.MapBankCategoryToLabel(bankQuestion.Category),
                Title = bankQuestion.Title,
                Description = bankQuestion.Content,
                // Bank questions don't carry a function name / test cases; the coach
                // completes those fields before "Send to Editor" becomes available.
                FunctionName = null,
                TestCases = null,
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
    }
}
