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
    public class SendPreparedQuestionToEditor(
        IUnitOfWork unitOfWork,
        IInterviewRoomRealtimePusher pusher,
        ILogger<SendPreparedQuestionToEditor> logger) : ISendPreparedQuestionToEditor
    {
        public async Task<PreparedQuestionDto> ExecuteAsync(Guid preparedQuestionId, Guid userId)
        {
            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();
            var entity = await preparedRepo.GetByIdAsync(preparedQuestionId)
                ?? throw new NotFoundException("Prepared question not found");

            if (entity.InteractionType != PreparedQuestionInteractionType.Coding)
            {
                throw new BadRequestException(
                    "Only coding questions can be sent to the editor. Use Mark as Asked for behavioral questions.");
            }

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            var room = await PreparedQuestionAuthorization.EnsureRoomCoachAsync(
                roomRepo, entity.InterviewRoomId, userId);

            var now = DateTime.UtcNow;
            var testCases = entity.TestCases ?? Array.Empty<object>();

            // Mirror the in-room QuestionPanel "Set Problem" flow: persist the room
            // problem fields so the candidate's next reconnect restores the same state.
            room.ProblemDescription = entity.Description;
            room.ProblemShortName = entity.FunctionName ?? string.Empty;
            room.TestCases = testCases;
            roomRepo.UpdateAsync(room);

            if (entity.Status != PreparedQuestionStatus.Asked)
            {
                entity.Status = PreparedQuestionStatus.Asked;
                entity.AskedAt = now;
            }
            entity.UpdatedAt = now;
            preparedRepo.UpdateAsync(entity);

            await unitOfWork.SaveChangesAsync();

            var dto = PreparedQuestionMapper.ToDto(entity);

            // Push ReceiveProblem + PreparedQuestionStatusChanged. Failures here are
            // logged but MUST NOT roll back the database change – the coach can re-send
            // from the Workspace if the broadcast drops.
            try
            {
                await pusher.PushProblemToRoomAsync(
                    entity.InterviewRoomId,
                    excludeUserId: userId,
                    description: entity.Description,
                    shortName: entity.FunctionName ?? string.Empty,
                    testCases: testCases);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to broadcast ReceiveProblem for {PreparedQuestionId}",
                    preparedQuestionId);
            }

            try
            {
                await pusher.PushPreparedQuestionStatusChangedAsync(entity.InterviewRoomId, dto);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to push PreparedQuestionStatusChanged (send-to-editor) for {PreparedQuestionId}",
                    preparedQuestionId);
            }

            return dto;
        }
    }
}
