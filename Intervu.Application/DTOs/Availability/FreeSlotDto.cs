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
        /// For merged slots that span multiple availability records, this is Guid.Empty
        /// and AvailabilityIds should be used instead.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Source CoachAvailability IDs that contributed to this free block.
        /// Contains one item for simple slots and multiple items for merged slots.
        /// </summary>
        public List<Guid> AvailabilityIds { get; set; } = new();

        public Guid CoachId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// Always 0 (Available) since we only return bookable time.
        /// </summary>
        public int Status { get; set; } = 0;
    }
}
