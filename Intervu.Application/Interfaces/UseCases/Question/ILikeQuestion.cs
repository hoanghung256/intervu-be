using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Question
{
    public interface ILikeQuestion
    {
        /// <summary>Toggles a like. Returns true if liked, false if unliked.</summary>
        Task<bool> ExecuteAsync(Guid questionId, Guid userId);
    }
}
