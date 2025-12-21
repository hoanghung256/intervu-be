using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerUpdateDto
    {

        //User properties
        public Guid Id { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string? PortfolioUrl { get; set; }
        public int? CurrentAmount { get; set; }
        public int? ExperienceYears { get; set; }

        public string? Bio { get; set; }

        public string? BankBinNumber { get; set; }

        public string? BankAccountNumber { get; set; }

        public List<Guid> CompanyIds { get; set; }

        public List<Guid> SkillIds { get; set; }
    }
}
