using Intervu.Domain.Abstractions.Entity;
using System;
using System.Collections.Generic;

namespace Intervu.Domain.Entities
{
    public class CandidateProfile : EntityBase<Guid>
    {
        /// <summary>
        /// References User.Id (Candidate)
        /// EntityBase.Id will be used as CandidateId
        /// </summary>
        public User User { get; set; } = null!;

        public string? CVUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();

        public ICollection<Industry> Industries { get; set; } = new List<Industry>();

        public string? Bio { get; set; }

        public int CurrentAmount { get; set; }

        public ICollection<CandidateCertificate> Certificates { get; set; } = new List<CandidateCertificate>();

        public ICollection<CandidateWorkExperience> WorkExperiences { get; set; } = new List<CandidateWorkExperience>();

        public string BankBinNumber { get; set; } = string.Empty;

        public string BankAccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// JSON list of saved question snapshots. Nullable.
        /// </summary>
        public List<QuestionSnapshot>? SavedQuestions { get; set; }
    }
}
