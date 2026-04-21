using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.PreparedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities.Constants.PreparedQuestionConstants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.PreparedQuestion
{
    public class UnmarkPreparedQuestionAsked(
        IUnitOfWork unitOfWork,
        IInterviewRoomRealtimePusher pusher,
        ILogger<UnmarkPreparedQuestionAsked> logger) : IUnmarkPreparedQuestionAsked
    {
        public async Task<PreparedQuestionDto> ExecuteAsync(Guid preparedQuestionId, Guid userId)
        {
            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();
            var entity = await preparedRepo.GetByIdAsync(preparedQuestionId)
                ?? throw new NotFoundException("Prepared question not found");

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, entity.InterviewRoomId, userId);

            if (entity.Status == PreparedQuestionStatus.Pending)
            {
                return PreparedQuestionMapper.ToDto(entity);
            }

            entity.Status = PreparedQuestionStatus.Pending;
            entity.AskedAt = null;
            entity.UpdatedAt = DateTime.UtcNow;

            preparedRepo.UpdateAsync(entity);
            await unitOfWork.SaveChangesAsync();

            var dto = PreparedQuestionMapper.ToDto(entity);

            try
            {
                await pusher.PushPreparedQuestionStatusChangedAsync(entity.InterviewRoomId, dto);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to push PreparedQuestionStatusChanged (unmark) for {PreparedQuestionId}",
                    preparedQuestionId);
            }

            return dto;
        }
    }
}
