using System;

namespace Intervu.Domain.Entities
{
    public class UserAssessmentAnswer
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AssessmentId { get; set; }
        public string QuestionId { get; set; } = null!;
        public string Skill { get; set; } = null!;
        public string? Answer { get; set; }
        public string SelectedLevel { get; set; } = "None";
        public int SfiaLevel { get; set; }
        public string Type { get; set; } = "Survey";
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
    }
}
