using System;

namespace Intervu.Application.DTOs.Availability
{
    public class CoachAvailabilityUpdateDto
    {
        public Guid CoachId { get; set; }
        public DateTimeOffset OriginalStartTime { get; set; }
        public DateTimeOffset OriginalEndTime { get; set; }
        public DateTimeOffset NewStartTime { get; set; }
        public DateTimeOffset NewEndTime { get; set; }
    }
}
