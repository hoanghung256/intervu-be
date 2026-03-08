using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Comment
{
    public interface ILikeComment
    {
        /// <summary>Toggles a like. Returns true if liked, false if unliked.</summary>
        Task<bool> ExecuteAsync(Guid commentId, Guid userId);
    }
}
