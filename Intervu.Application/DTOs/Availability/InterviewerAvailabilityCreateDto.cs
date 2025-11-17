using System;

namespace Intervu.Application.DTOs.Availability
{
    public class InterviewerAvailabilityCreateDto
    {
        public int InterviewerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}