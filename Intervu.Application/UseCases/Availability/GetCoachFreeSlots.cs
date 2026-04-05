using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Services;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    /// <summary>
    /// Returns computed free time slots for a coach by subtracting active bookings
    /// (both direct and JD flows) from available blocks.
    /// The frontend receives these to display bookable time slots.
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
            // 1. Get all availability blocks for the coach
            var availabilities = (await _availabilityRepo
                .GetCoachAvailabilitiesByMonthAsync(coachId, month, year))
                .Where(a => a.Status == CoachAvailabilityStatus.Available)
                .ToList();

            // 2. Get confirmed bookings from both direct booking and JD booking flows
            var directBookings = await _transactionRepo.GetConfirmedBookingEntitiesForCoachAsync(coachId, month, year);
            var jdBookings = await _bookingRequestRepo.GetConfirmedBookingEntitiesForCoachAsync(coachId, month, year);

            var allBookedIntervals = directBookings
                .Select(b => (b.BookedStartTime!.Value, b.BookedStartTime!.Value.AddMinutes(b.BookedDurationMinutes!.Value)))
                .Concat(jdBookings.Select(b => (b.StartTime, b.EndTime)))
                .ToList();

            // 3. Calculate free slots by subtracting bookings from availabilities
            var freeTimeSlots = AvailabilityCalculatorService.CalculateFreeSlots(availabilities, allBookedIntervals);

            // 4. Split free ranges back into 30-min blocks (the frontend expects individual blocks)
            const int blockMinutes = 30;
            var result = new List<FreeSlotDto>();
            foreach (var slot in freeTimeSlots)
            {
                var blockStart = slot.Start;
                while (blockStart.AddMinutes(blockMinutes) <= slot.End)
                {
                    var blockEnd = blockStart.AddMinutes(blockMinutes);
                    var parentAvailability = availabilities.FirstOrDefault(a => blockStart >= a.StartTime && blockEnd <= a.EndTime);
                    result.Add(new FreeSlotDto
                    {
                        Id = parentAvailability?.Id ?? Guid.Empty,
                        CoachId = coachId,
                        StartTime = blockStart,
                        EndTime = blockEnd
                    });
                    blockStart = blockEnd;
                }
            }

            return result;
        }
    }
}
