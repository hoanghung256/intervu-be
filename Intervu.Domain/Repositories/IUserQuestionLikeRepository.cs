using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IUserQuestionLikeRepository
    {
        /// <summary>Returns true if the like was added, false if it was removed (toggle).</summary>
        Task<bool> ToggleAsync(Guid userId, Guid questionId);

        /// <summary>Returns the subset of questionIds the user has liked.</summary>
        Task<HashSet<Guid>> GetLikedQuestionIdsAsync(Guid userId, IEnumerable<Guid> questionIds);

        Task<bool> HasLikedAsync(Guid userId, Guid questionId);

        Task SaveChangesAsync();
    }
}
