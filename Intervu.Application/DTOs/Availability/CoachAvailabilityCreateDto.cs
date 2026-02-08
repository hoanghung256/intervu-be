using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Application.DTOs.Availability
{
    public class CoachAvailabilityCreateDto
    {
        public Guid CoachId { get; set; }
        public InterviewFocus Focus { get; set; } = InterviewFocus.General_Skills;
        public Guid? TypeId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}