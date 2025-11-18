using System;

namespace Intervu.Application.DTOs.Availability
{
    public class InterviewerAvailabilityCreateDto
    {
        public int InterviewerId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}