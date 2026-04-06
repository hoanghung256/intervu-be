using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Services
{
    /// <summary>
    /// Represents a contiguous block of free time within a coach's availability.
    /// </summary>
    public record TimeSlot(DateTime Start, DateTime End)
    {
        public TimeSpan Duration => End - Start;
    }

    /// <summary>
    /// Calculates actual free (un-booked) time slots by subtracting confirmed bookings
    /// from the coach's published availability ranges.
    /// </summary>
    public static class AvailabilityCalculatorService
    {
        /// <summary>
        /// Subtracts a list of booked (Start, End) intervals from availability windows,
        /// returning the remaining free time slots.
        /// </summary>
        public static List<TimeSlot> CalculateFreeSlots(
            List<CoachAvailability> availabilities,
            List<(DateTime Start, DateTime End)> bookedIntervals)
        {
            if (availabilities is null || availabilities.Count == 0)
                return [];

            var activeBookings = (bookedIntervals ?? [])
                .OrderBy(b => b.Start)
                .ToList();

            var freeSlots = new List<TimeSlot>();

            // ── Step 2: Process each availability window ───────────────
            foreach (var avail in availabilities.OrderBy(a => a.StartTime))
            {
                // Only consider windows that are marked Available
                if (avail.Status != CoachAvailabilityStatus.Available)
                    continue;

                var windowStart = avail.StartTime;
                var windowEnd = avail.EndTime;

                // Find bookings that overlap [windowStart, windowEnd)
                var overlapping = activeBookings
                    .Where(b => b.Start < windowEnd && b.End > windowStart)
                    .Select(b => (
                        // Clamp booking edges to the window boundaries
                        Start: b.Start < windowStart ? windowStart : b.Start,
                        End: b.End > windowEnd ? windowEnd : b.End
                    ))
                    .OrderBy(b => b.Start)
                    .ToList();

                // ── Step 3: Merge overlapping / adjacent bookings ──────
                var merged = MergeIntervals(overlapping);

                // ── Step 4: Walk cursor and emit free gaps ─────────────
                var cursor = windowStart;

                foreach (var booking in merged)
                {
                    // Gap between cursor and the next booking
                    if (booking.Start > cursor)
                    {
                        freeSlots.Add(new TimeSlot(cursor, booking.Start));
                    }

                    // Advance cursor past this booking
                    if (booking.End > cursor)
                        cursor = booking.End;
                }

                // Remaining gap after last booking
                if (cursor < windowEnd)
                {
                    freeSlots.Add(new TimeSlot(cursor, windowEnd));
                }
            }

            // ── Step 5: Merge overlapping free slots and return ──────────
            // When availability windows overlap, the per-window pass can
            // produce duplicate / overlapping free slots.  Merge them so
            // the frontend never renders stacked semi-transparent events.
            var sorted = freeSlots
                .Where(s => s.Duration > TimeSpan.Zero)
                .OrderBy(s => s.Start)
                .ToList();

            var mergedFree = MergeIntervals(
                sorted.Select(s => (s.Start, s.End)).ToList());

            return mergedFree
                .Select(m => new TimeSlot(m.Start, m.End))
                .ToList();
        }

        /// <summary>
        /// Merges a sorted list of intervals so that overlapping or adjacent
        /// intervals become a single continuous range.
        /// Input MUST be pre-sorted by Start.
        /// </summary>
        private static List<(DateTime Start, DateTime End)> MergeIntervals(
            List<(DateTime Start, DateTime End)> intervals)
        {
            if (intervals.Count == 0)
                return [];

            var result = new List<(DateTime Start, DateTime End)> { intervals[0] };

            for (int i = 1; i < intervals.Count; i++)
            {
                var last = result[^1];
                var current = intervals[i];

                if (current.Start <= last.End)
                {
                    // Overlapping or adjacent → extend the end if needed
                    result[^1] = (last.Start, current.End > last.End ? current.End : last.End);
                }
                else
                {
                    result.Add(current);
                }
            }

            return result;
        }
    }
}
