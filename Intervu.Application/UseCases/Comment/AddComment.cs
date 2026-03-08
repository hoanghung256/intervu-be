using Intervu.Application.DTOs.Comment;
using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Comment
{
    public class AddComment(IUnitOfWork unitOfWork) : IAddComment
    {
        public async Task<Guid> ExecuteAsync(Guid questionId, CreateCommentRequest request, Guid userId)
        {
            var commentRepo = unitOfWork.GetRepository<ICommentRepository>();

            var comment = new Domain.Entities.Comment
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                Content = request.Content,
                IsAnswer = false,
                Vote = 0,
                CreatedAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                CreateBy = userId,
                UpdateBy = userId
            };

            await commentRepo.AddAsync(comment);
            await unitOfWork.SaveChangesAsync();

            return comment.Id;
        }
    }
}
