using System;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Tracks which users have liked which comments (composite PK: UserId + CommentId).
    /// </summary>
    public class UserCommentLike
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
