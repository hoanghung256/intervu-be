using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.PreparedQuestion;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.PreparedQuestion
{
    public class ReorderPreparedQuestions(IUnitOfWork unitOfWork) : IReorderPreparedQuestions
    {
        public async Task ExecuteAsync(
            Guid interviewRoomId,
            ReorderPreparedQuestionsRequest request,
            Guid userId)
        {
            if (request == null || request.OrderedIds == null || request.OrderedIds.Count == 0)
            {
                throw new BadRequestException("orderedIds is required");
            }

            var roomRepo = unitOfWork.GetRepository<IInterviewRoomRepository>();
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(roomRepo, interviewRoomId, userId);

            var preparedRepo = unitOfWork.GetRepository<IPreparedQuestionRepository>();

            // Load every row owned by the room; we only reorder rows supplied in the
            // payload and leave any others untouched (in case the client is stale).
            var allInRoom = await preparedRepo.GetByInterviewRoomIdAsync(interviewRoomId);
            var lookup = allInRoom.ToDictionary(x => x.Id);

            var now = DateTime.UtcNow;
            var sortOrder = 0;
            foreach (var id in request.OrderedIds.Distinct())
            {
                if (!lookup.TryGetValue(id, out var entity))
                {
                    continue;
                }

                if (entity.SortOrder != sortOrder)
                {
                    entity.SortOrder = sortOrder;
                    entity.UpdatedAt = now;
                    preparedRepo.UpdateAsync(entity);
                }

                sortOrder++;
            }

            await unitOfWork.SaveChangesAsync();
        }
    }
}
