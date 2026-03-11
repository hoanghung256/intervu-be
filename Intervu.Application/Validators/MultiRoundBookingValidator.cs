using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;

namespace Intervu.Application.Validators
{
    public static class MultiRoundBookingValidator
    {
        private static readonly TimeSpan MinGapBetweenRounds = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Validates a JD multi-round booking request against the coach's computed free slots.
        /// Throws <see cref="BadRequestException"/> on any violation.
        /// </summary>
        /// <param name="request">The booking request containing rounds.</param>
        /// <param name="availableSlots">Computed free slots (after subtracting existing bookings).</param>
        /// <param name="serviceDurations">Map of CoachInterviewServiceId → duration in minutes.</param>
        public static void ValidateMultiRoundBooking(
            CreateJDBookingRequestDto request,
            List<FreeSlotDto> availableSlots,
            Dictionary<Guid, int> serviceDurations)
        {
            // Rule 1: At least 2 rounds
            if (request.Rounds == null || request.Rounds.Count < 2)
                throw new BadRequestException("At least 2 rounds are required for JD multi-round interviews");

            // Rule 2: Sort chronologically
            var orderedRounds = request.Rounds.OrderBy(r => r.StartTime).ToList();

            // Validate all start times are in the future
            if (orderedRounds.Any(r => r.StartTime <= DateTime.UtcNow))
                throw new BadRequestException("All round start times must be in the future");

            // Resolve durations and compute end times
            var roundTimeRanges = new List<(int Index, DateTime Start, DateTime End, int Duration)>();

            for (int i = 0; i < orderedRounds.Count; i++)
            {
                var round = orderedRounds[i];

                if (!serviceDurations.TryGetValue(round.CoachInterviewServiceId, out var duration))
                    throw new BadRequestException(
                        $"Round {i + 1}: service {round.CoachInterviewServiceId} not found or does not belong to this coach");

                var endTime = round.StartTime.AddMinutes(duration);
                roundTimeRanges.Add((i, round.StartTime, endTime, duration));
            }

            // Rule 3: Every round must fit within at least one free slot
            foreach (var (index, start, end, duration) in roundTimeRanges)
            {
                var fitsInSlot = availableSlots.Any(slot =>
                    start >= slot.StartTime && end <= slot.EndTime);

                if (!fitsInSlot)
                    throw new BadRequestException(
                        $"Round {index + 1} ({start:g} – {end:g}) does not fit within any of the coach's available time slots");
            }

            // Rule 4: 15-minute gap between consecutive rounds
            for (int i = 1; i < roundTimeRanges.Count; i++)
            {
                var prev = roundTimeRanges[i - 1];
                var curr = roundTimeRanges[i];

                if (curr.Start < prev.End.Add(MinGapBetweenRounds))
                    throw new BadRequestException(
                        $"Round {i + 1} must start at least 15 minutes after round {i} ends. " +
                        $"Round {i} ends at {prev.End:g}, round {i + 1} starts at {curr.Start:g}");
            }
        }
    }
}
