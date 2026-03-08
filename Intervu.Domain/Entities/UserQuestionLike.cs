using System;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Tracks which users have liked which questions (composite PK: UserId + QuestionId).
    /// </summary>
    public class UserQuestionLike
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
