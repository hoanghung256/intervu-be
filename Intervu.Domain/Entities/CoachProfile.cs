using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;

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

        public int? ExperienceYears { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string BankBinNumber { get; set; } = string.Empty;

        public string BankAccountNumber { get; set; } = string.Empty;

        public CoachProfileStatus Status { get; set; }

        
        public ICollection<Company> Companies { get; set; } = new List<Company>();
        
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
