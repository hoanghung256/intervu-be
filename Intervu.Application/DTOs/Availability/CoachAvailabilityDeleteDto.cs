using System;

namespace Intervu.Application.DTOs.Availability
{
    public class CoachAvailabilityDeleteDto
    {
        public Guid CoachId { get; set; }
        public DateTimeOffset RangeStartTime { get; set; }
        public DateTimeOffset RangeEndTime { get; set; }
    }
}
