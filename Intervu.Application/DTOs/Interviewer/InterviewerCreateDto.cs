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

        public int CurrentAmount { get; set; }

        public int ExperienceYears { get; set; }

        /// <summary>
        /// Created automatically pending status 
        /// waiting for approvement from admin
        /// </summary>
        /// 

        public InterviewerProfileStatus Status { get; set; } = InterviewerProfileStatus.Enable;

        public List<int> CompanyIds { get; set; }

        public List<int> SkillIds { get; set; }
    }
}
