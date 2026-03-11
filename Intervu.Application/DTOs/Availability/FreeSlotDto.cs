namespace Intervu.Application.DTOs.Availability
{
    /// <summary>
    /// Represents a single contiguous block of free (bookable) time
    /// within a coach's availability, after subtracting existing bookings.
    /// Designed to be a drop-in replacement for the raw CoachAvailability shape
    /// so the frontend can consume it without changes.
    /// </summary>
    public class FreeSlotDto
    {
        /// <summary>
        /// The original CoachAvailability Id that contains this free block.
        /// The candidate sends this back as coachAvailabilityId when booking.
        /// </summary>
        public Guid Id { get; set; }

        public Guid CoachId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// Always 0 (Available) since we only return bookable time.
        /// </summary>
        public int Status { get; set; } = 0;
    }
}
