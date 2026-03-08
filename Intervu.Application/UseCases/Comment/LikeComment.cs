using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Comment
{
    public class LikeComment(IUserCommentLikeRepository likeRepository) : ILikeComment
    {
        public async Task<bool> ExecuteAsync(Guid commentId, Guid userId)
        {
            return await likeRepository.ToggleAsync(userId, commentId);
        }
    }
}
