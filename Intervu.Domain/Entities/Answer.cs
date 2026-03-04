using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
        public class Answer : EntityBase<Guid>
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public string Content { get; set; } = null!;

        public int Upvotes { get; set; } = 0;

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
