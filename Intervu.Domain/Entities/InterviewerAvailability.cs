using Intervu.Domain.Abstractions.Entities;
using System;

namespace Intervu.Domain.Entities
{
    public class InterviewerAvailability : EntityBase<int>
    {
        /// <summary>
        /// EntityBase.Id represents AvailabilityId
        /// References InterviewerProfile.Id
        /// </summary>
        public int InterviewerId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}
