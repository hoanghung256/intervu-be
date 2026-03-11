using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Comment
{
    public interface IDeleteComment
    {
        Task ExecuteAsync(Guid commentId, Guid userId);
    }
}
