using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.PreparedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants.PreparedQuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.PreparedQuestion
{
    public class DeletePreparedQuestion(IUnitOfWork unitOfWork) : IDeletePreparedQuestion
    {
        public async Task ExecuteAsync(Guid preparedQuestionId, Guid userId)
        {
            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();
            var entity = await preparedRepo.GetByIdAsync(preparedQuestionId)
                ?? throw new NotFoundException("Prepared question not found");

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, entity.InterviewRoomId, userId);

            // Preserve audit trail: once a question has been asked in the live room we
            // don't allow hard deletes from the Workspace. Coaches can un-mark instead.
            if (entity.Status == PreparedQuestionStatus.Asked)
            {
                throw new ConflictException(
                    "This question has already been asked. Unmark it before removing it from the list.");
            }

            preparedRepo.DeleteAsync(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
