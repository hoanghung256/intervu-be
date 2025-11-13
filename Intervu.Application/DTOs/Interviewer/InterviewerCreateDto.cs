using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerCreateDto
    {
        public int Id { get; set; }
        public string CVUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public string Specializations { get; set; }
        public string ProgrammingLanguages { get; set; }

        public string Company { get; set; }

        public int CurrentAmount { get; set; }

        public int ExperienceYears { get; set; }

        public string Bio { get; set; }

        /// <summary>
        /// Created automatically pending status 
        /// waiting for approvement from admin
        /// </summary>
        public InterviewerProfileStatus Status { get; set; } = InterviewerProfileStatus.Pending;

    }
}
