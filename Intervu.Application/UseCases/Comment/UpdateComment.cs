using Intervu.Application.DTOs.Comment;
using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Comment
{
    public class UpdateComment(IUnitOfWork unitOfWork) : IUpdateComment
    {
        public async Task ExecuteAsync(Guid commentId, UpdateCommentRequest request, Guid userId)
        {
            var commentRepo = unitOfWork.GetRepository<ICommentRepository>();
            var comment = await commentRepo.GetByIdAsync(commentId)
                ?? throw new Exception("Comment not found");

            comment.Content = request.Content;
            comment.UpdateAt = DateTime.UtcNow;
            comment.UpdateBy = userId;

            commentRepo.UpdateAsync(comment);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task VoteAsync(Guid questionId, bool isUpvote, Guid userId)
        {
            var commentRepo = unitOfWork.GetRepository<ICommentRepository>();
            var comment = await commentRepo.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");
            if (isUpvote)
            {
                comment.Vote++;
            }
            else comment.Vote--;
            commentRepo.UpdateAsync(comment);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
