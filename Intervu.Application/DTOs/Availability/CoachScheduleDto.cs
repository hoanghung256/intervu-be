using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Availability
{
    /// <summary>
    /// Represents a coach's complete schedule for a given period,
    /// including both available time slots and confirmed bookings.
    /// </summary>
    public class CoachScheduleDto
    {
        public List<FreeSlotDto> FreeSlots { get; set; } = new();
        public List<BookedSlotDto> BookedSlots { get; set; } = new();
    }

    /// <summary>
    /// Represents a confirmed booking in a coach's schedule.
    /// </summary>
    public class BookedSlotDto
    {
        public Guid BookingId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string InterviewType { get; set; } = string.Empty; // e.g., "Mock Interview", "Resume Review"
        public string Status { get; set; } = string.Empty; // e.g., "Confirmed", "Paid"
    }
}
