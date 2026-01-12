using System;

namespace Intervu.Application.DTOs.Availability
{
    public class CoachAvailabilityUpdateDto
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
