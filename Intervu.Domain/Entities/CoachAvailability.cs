using Intervu.Domain.Abstractions.Entity;
using System;

namespace Intervu.Domain.Entities
{
    public class CoachAvailability : EntityBase<Guid>
    {
        /// <summary>
        /// EntityBase.Id represents AvailabilityId
        /// References CoachProfile.Id
        /// </summary>
        public Guid CoachId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}
