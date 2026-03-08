using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Services;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    /// <summary>
    /// Subtraction-pattern query: fetches coach availabilities for a month,
    /// fetches active bookings (transactions + booking-request rounds) in the same range,
    /// then uses <see cref="AvailabilityCalculatorService"/> to compute the actual free slots.
    /// Returns <see cref="FreeSlotDto"/> objects shaped like CoachAvailability
    /// so the frontend can consume them without changes.
    /// </summary>
    public class GetCoachFreeSlots : IGetCoachFreeSlots
    {
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IBookingRequestRepository _bookingRequestRepo;

        public GetCoachFreeSlots(
            ICoachAvailabilitiesRepository availabilityRepo,
            ITransactionRepository transactionRepo,
            IBookingRequestRepository bookingRequestRepo)
        {
            _availabilityRepo = availabilityRepo;
            _transactionRepo = transactionRepo;
            _bookingRequestRepo = bookingRequestRepo;
        }

        public async Task<List<FreeSlotDto>> ExecuteAsync(Guid coachId, int month = 0, int year = 0)
        {
            // 1. Get raw availability windows for the requested period
            var availabilities = (await _availabilityRepo
                .GetCoachAvailabilitiesByMonthAsync(coachId, month, year))
                .ToList();

            if (availabilities.Count == 0)
                return [];

            // 2. Determine the date range covered by these availabilities
            var rangeStart = availabilities.Min(a => a.StartTime);
            var rangeEnd = availabilities.Max(a => a.EndTime);

            // 3. Fetch active bookings from BOTH sources:
            //    a) Flow A transactions (direct bookings with payment)
            var activeTransactions = await _transactionRepo
                .GetActiveBookingsByCoachAsync(coachId, rangeStart, rangeEnd);
            //    b) Flow C booking-request rounds (Pending/Accepted/Paid)
            var activeRounds = await _bookingRequestRepo
                .GetActiveRoundsByCoachAsync(coachId, rangeStart, rangeEnd);

            // Merge both sources into unified (Start, End) intervals
            var allBookedIntervals = activeTransactions
                .Where(t => t.BookedStartTime.HasValue && t.BookedDurationMinutes.HasValue)
                .Select(t => (
                    Start: t.BookedStartTime!.Value,
                    End: t.BookedStartTime!.Value.AddMinutes(t.BookedDurationMinutes!.Value)
                ))
                .Concat(activeRounds)
                .ToList();

            // 4. Compute free slots using the subtraction algorithm
            var freeSlots = AvailabilityCalculatorService
                .CalculateFreeSlots(availabilities, allBookedIntervals);

            // 5. Map each free TimeSlot back to the original CoachAvailability it belongs to,
            //    so the frontend gets a valid coachAvailabilityId to send when booking.
            var result = new List<FreeSlotDto>();

            foreach (var slot in freeSlots)
            {
                // Find the availability window that contains this free slot
                var parent = availabilities.FirstOrDefault(a =>
                    a.StartTime <= slot.Start && a.EndTime >= slot.End);

                if (parent == null) continue; // safety — should not happen

                result.Add(new FreeSlotDto
                {
                    Id = parent.Id,
                    CoachId = parent.CoachId,
                    StartTime = slot.Start,
                    EndTime = slot.End,
                    Status = 0, // Available
                });
            }

            return result;
        }
    }
}
