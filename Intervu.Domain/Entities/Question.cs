using System;

namespace Intervu.Domain.Entities
{
    public class Question
    {
        public Guid Id { get; set; }

        public Guid InterviewExperienceId { get; set; }

        public string QuestionType { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string? Answer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public InterviewExperience InterviewExperience { get; set; } = null!;

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
