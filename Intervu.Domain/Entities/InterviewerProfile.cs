using Intervu.Domain.Abstractions.Entities;
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
        public string CVUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        /// <summary>
        /// e.g. Backend, Frontend, ... stored as text or CSV/JSON
        /// </summary>
        public string Specializations { get; set; }

        public string ProgrammingLanguages { get; set; }

        public string Company { get; set; }

        public int CurrentAmount { get; set; }

        public int ExperienceYears { get; set; }

        public string Bio { get; set; }

        public InterviewerProfileStatus Status { get; set; }


    }
}
