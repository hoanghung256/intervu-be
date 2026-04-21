using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Interfaces.UseCases.PreparedQuestion;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.PreparedQuestion
{
    public class GetPreparedQuestions(
        IPreparedQuestionRepository preparedQuestionRepository,
        IInterviewRoomRepository interviewRoomRepository) : IGetPreparedQuestions
    {
        public async Task<List<PreparedQuestionDto>> ExecuteAsync(Guid interviewRoomId, Guid userId)
        {
            await PreparedQuestionAuthorization.EnsureRoomCoachAsync(
                interviewRoomRepository, interviewRoomId, userId);

            var items = await preparedQuestionRepository.GetByInterviewRoomIdAsync(interviewRoomId);
            return items.Select(PreparedQuestionMapper.ToDto).ToList();
        }
    }
}
