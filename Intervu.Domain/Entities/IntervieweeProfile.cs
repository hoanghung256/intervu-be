using Intervu.Domain.Abstractions.Entity;
using System;
using System.Collections.Generic;

namespace Intervu.Domain.Entities
{
    public class IntervieweeProfile : EntityBase<Guid>
    {
        /// <summary>
        /// References User.Id (Interviewee)
        /// EntityBase.Id will be used as IntervieweeId
        /// </summary>
        public User User { get; set; } = null!;

        public string? CVUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();

        public string? Bio { get; set; }

        public int CurrentAmount { get; set; }
    }
}
