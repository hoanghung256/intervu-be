using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Comment
{
    public interface IGetComments
    {
        Task<PagedResult<CommentDetailDto>> ExecuteAsync(Guid questionId, int page, int pageSize);
    }
}
