using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;

namespace Intervu.Domain.Entities
{
    public class InterviewExperience
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public string Role { get; set; } = null!;

        public ExperienceLevel? Level { get; set; }

        public string LastRoundCompleted { get; set; } = null!;

        public string InterviewProcess { get; set; } = null!;

        public bool IsInterestedInContact { get; set; } = false;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
