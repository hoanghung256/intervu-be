using Intervu.Application.DTOs.Comment;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Comment
{
    public interface IUpdateComment
    {
        Task ExecuteAsync(Guid commentId, UpdateCommentRequest request, Guid userId);
        Task VoteAsync(Guid commentId, bool isUpvote, Guid userId);
    }
}
