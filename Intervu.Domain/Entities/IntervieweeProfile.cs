using Intervu.Domain.Abstractions.Entities;
using System;

namespace Intervu.Domain.Entities
{
    public class IntervieweeProfile : EntityBase<int>
    {
        /// <summary>
        /// References User.Id (Interviewee)
        /// EntityBase.Id will be used as IntervieweeId
        /// </summary>
        public string CVUrl { get; set; }

        public string PortfolioUrl { get; set; }

        /// <summary>
        /// Stored as text or JSON string
        /// </summary>
        public string Skills { get; set; }

        public string Bio { get; set; }

        public int CurrentAmount { get; set; }

    }
}
