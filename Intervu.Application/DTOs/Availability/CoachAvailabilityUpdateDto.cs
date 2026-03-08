using System;

namespace Intervu.Application.DTOs.Availability
{
    public class CoachAvailabilityUpdateDto
    {
        public Guid CoachId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
