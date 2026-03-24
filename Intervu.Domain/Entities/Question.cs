using Intervu.Domain.Entities.Constants.QuestionConstants;
using System;
using System.Collections.Generic;

namespace Intervu.Domain.Entities
{
    public class Question
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public Guid? InterviewExperienceId { get; set; }

        public ExperienceLevel Level { get; set; }

        public Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound Round { get; set; }

        public QuestionStatus Status { get; set; } = QuestionStatus.Approved;

        public int ViewCount { get; set; } = 0;
        public int Vote { get; set; } = 0;

        public int SaveCount { get; set; } = 0;

        public bool IsHot { get; set; } = false;

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public InterviewExperience? InterviewExperience { get; set; }

        public User? Author { get; set; }

        public ICollection<QuestionCompany> QuestionCompanies { get; set; } = new List<QuestionCompany>();

        public ICollection<QuestionRole> QuestionRoles { get; set; } = new List<QuestionRole>();

        public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();

        public QuestionCategory Category { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<QuestionReport> Reports { get; set; } = new List<QuestionReport>();
    }
}
