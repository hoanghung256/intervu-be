using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class LikeQuestion(IUserQuestionLikeRepository likeRepository) : ILikeQuestion
    {
        public async Task<bool> ExecuteAsync(Guid questionId, Guid userId)
        {
            return await likeRepository.ToggleAsync(userId, questionId);
        }
    }
}
