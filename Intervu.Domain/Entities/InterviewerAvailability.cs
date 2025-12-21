using Intervu.Domain.Abstractions.Entity;
using System;

namespace Intervu.Domain.Entities
{
    public class InterviewerAvailability : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents AvailabilityId
        /// References InterviewerProfile.Id
        /// </summary>
        public Guid InterviewerId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}
