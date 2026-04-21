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
    public class MarkPreparedQuestionAsked(
        IUnitOfWork unitOfWork,
        IInterviewRoomRealtimePusher pusher,
        ILogger<MarkPreparedQuestionAsked> logger) : IMarkPreparedQuestionAsked
    {
        public async Task<PreparedQuestionDto> ExecuteAsync(Guid preparedQuestionId, Guid userId)
        {
            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();
            var entity = await preparedRepo.GetByIdAsync(preparedQuestionId)
                ?? throw new NotFoundException("Prepared question not found");

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, entity.InterviewRoomId, userId);

            if (entity.Status == PreparedQuestionStatus.Asked)
            {
                // Idempotent: return the existing state rather than error.
                return PreparedQuestionMapper.ToDto(entity);
            }

            entity.Status = PreparedQuestionStatus.Asked;
            entity.AskedAt = DateTime.UtcNow;
            entity.UpdatedAt = entity.AskedAt.Value;

            preparedRepo.UpdateAsync(entity);
            await unitOfWork.SaveChangesAsync();

            var dto = PreparedQuestionMapper.ToDto(entity);

            // Best-effort realtime notify so the coach's other tabs sync. Never block
            // the API response on a SignalR hiccup.
            try
            {
                await pusher.PushPreparedQuestionStatusChangedAsync(entity.InterviewRoomId, dto);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to push PreparedQuestionStatusChanged for {PreparedQuestionId}",
                    preparedQuestionId);
            }

            return dto;
        }
    }
}
