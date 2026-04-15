using System.ComponentModel.DataAnnotations;
using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;

namespace Intervu.Domain.Entities
{
    public class CoachProfile : EntityBase<Guid>
    {
        /// <summary>
        /// References User.Id (Interviewer)
        /// EntityBase.Id will be used as CoachId
        /// </summary>
        /// 

        public User? User { get; set; } 
        public string? PortfolioUrl { get; set; } = string.Empty;

        public int? CurrentAmount { get; set; }

        [ConcurrencyCheck]
        public int Version { get; set; }

        public int? ExperienceYears { get; set; }

        public string CurrentJobTitle { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public string BankBinNumber { get; set; } = string.Empty;

        public string BankAccountNumber { get; set; } = string.Empty;

        public CoachProfileStatus Status { get; set; }

        public ICollection<Company> Companies { get; set; } = new List<Company>();
        
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();

        public ICollection<Industry> Industries { get; set; } = new List<Industry>();

        public ICollection<CoachCertificate> Certificates { get; set; } = new List<CoachCertificate>();

        public ICollection<CoachWorkExperience> WorkExperiences { get; set; } = new List<CoachWorkExperience>();

        /// <summary>
        /// JSON list of saved question snapshots. Nullable.
        /// </summary>
        public List<QuestionSnapshot>? SavedQuestions { get; set; }
        
        public ICollection<CoachInterviewService> InterviewServices { get; set; } = new List<CoachInterviewService>();
    }
}
