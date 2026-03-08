using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Domain.Repositories
{
    public interface IUserCommentLikeRepository
    {
        /// <summary>Returns true if the like was added, false if it was removed (toggle).</summary>
        Task<bool> ToggleAsync(Guid userId, Guid commentId);

        /// <summary>Returns the subset of commentIds the user has liked.</summary>
        Task<HashSet<Guid>> GetLikedCommentIdsAsync(Guid userId, IEnumerable<Guid> commentIds);

        Task<bool> HasLikedAsync(Guid userId, Guid commentId);

        Task SaveChangesAsync();
    }
}
