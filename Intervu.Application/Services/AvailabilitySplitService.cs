using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Services
{
    /// <summary>
    /// Handles splitting a CoachAvailability range when a booking is confirmed.
    /// Applies a 15-minute buffer (break time) after the booked interview slot.
    /// </summary>
    public static class AvailabilitySplitService
    {
        private static readonly TimeSpan BufferTime = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Splits a CoachAvailability around a booked time range.
        /// The original record should be deleted after calling this method,
        /// and the returned ranges (0-2) should be inserted as new records.
        /// </summary>
        /// <param name="original">The availability range that contains the booking.</param>
        /// <param name="bookingStart">The start time of the booked interview.</param>
        /// <param name="bookingEnd">The end time of the booked interview (StartTime + Duration).</param>
        /// <returns>Tuple of (Before, After) availability ranges. Either or both can be null.</returns>
        public static (CoachAvailability? Before, CoachAvailability? After) Split(
            CoachAvailability original,
            DateTime bookingStart,
            DateTime bookingEnd)
        {
            CoachAvailability? before = null;
            CoachAvailability? after = null;

            // Range 1: [OriginalStart, BookingStart)
            // Only create if there's meaningful time before the booking
            if (bookingStart > original.StartTime)
            {
                before = new CoachAvailability
                {
                    Id = Guid.NewGuid(),
                    CoachId = original.CoachId,
                    StartTime = original.StartTime,
                    EndTime = bookingStart,
                    Status = CoachAvailabilityStatus.Available
                };
            }

            // Range 2: [BookingEnd + 15min buffer, OriginalEnd)
            // Only create if there's meaningful time after the booking + buffer
            var afterStart = bookingEnd + BufferTime;
            if (afterStart < original.EndTime)
            {
                after = new CoachAvailability
                {
                    Id = Guid.NewGuid(),
                    CoachId = original.CoachId,
                    StartTime = afterStart,
                    EndTime = original.EndTime,
                    Status = CoachAvailabilityStatus.Available
                };
            }

            return (before, after);
        }
    }
}
