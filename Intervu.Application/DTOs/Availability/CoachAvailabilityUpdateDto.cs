using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Application.DTOs.Availability
{
    public class CoachAvailabilityUpdateDto
    {
        public InterviewFocus Focus { get; set; } = InterviewFocus.General_Skills;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public Guid? TypeId { get; set; } 
    }
}
