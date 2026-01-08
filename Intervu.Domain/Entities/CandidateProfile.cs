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

        public string? Bio { get; set; }

        public int CurrentAmount { get; set; }
    }
}
