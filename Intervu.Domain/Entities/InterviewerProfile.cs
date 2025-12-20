using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Domain.Entities
{
    public class InterviewerProfile : EntityBase<int>
    {
        /// <summary>
        /// References User.Id (Interviewer)
        /// EntityBase.Id will be used as InterviewerId
        /// </summary>
        /// 

        public User User { get; set; } 
        public string? PortfolioUrl { get; set; } = string.Empty;

        public int? CurrentAmount { get; set; }

        public int? ExperienceYears { get; set; }

        public string Bio { get; set; } = string.Empty;

        public string BankBinNumber { get; set; } = string.Empty;

        public string BankAccountNumber { get; set; } = string.Empty;

        public InterviewerProfileStatus Status { get; set; }

        
        public ICollection<Company> Companies { get; set; } = new List<Company>();
        
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
