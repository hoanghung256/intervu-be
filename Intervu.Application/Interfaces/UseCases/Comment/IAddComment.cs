using Intervu.Application.DTOs.Comment;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Comment
{
    public interface IAddComment
    {
        Task<Guid> ExecuteAsync(Guid questionId, CreateCommentRequest request, Guid userId);
    }
}
