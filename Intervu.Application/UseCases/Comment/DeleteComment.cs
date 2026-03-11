using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Comment
{
    public class DeleteComment(IUnitOfWork unitOfWork) : IDeleteComment
    {
        public async Task ExecuteAsync(Guid commentId, Guid userId)
        {
            var commentRepo = unitOfWork.GetRepository<ICommentRepository>();
            var comment = await commentRepo.GetByIdAsync(commentId)
                ?? throw new Exception("Comment not found");

            commentRepo.DeleteAsync(comment);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
