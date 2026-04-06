using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Services;

namespace Intervu.Application.UseCases.Availability
{
    public class GetCoachAvailabilities : IGetCoachAvailabilities
    {
        private readonly ICoachAvailabilitiesRepository _coachAvailabilitiesRepository;
        private readonly IBookingRequestRepository _bookingRequestRepository;

        public GetCoachAvailabilities(
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            IBookingRequestRepository bookingRequestRepository)
        {
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _bookingRequestRepository = bookingRequestRepository;
        }

        public async Task<CoachScheduleDto> ExecuteAsync(Guid coachId, int month = 0, int year = 0)
        {
            // 1. Get all availability windows for the coach in the given month
            var availabilities = await _coachAvailabilitiesRepository.GetCoachAvailabilitiesByMonthAsync(coachId, month, year);
            var availabilityList = availabilities.ToList();

            // 2. Get all confirmed bookings (unified through rounds)
            var allBookedIntervals = await _bookingRequestRepository.GetConfirmedBookingsForCoachAsync(coachId, month, year);
            var bookingEntities = await _bookingRequestRepository.GetConfirmedBookingEntitiesForCoachAsync(coachId, month, year);

            // 3. Calculate the actual free slots by subtracting bookings from availabilities
            var freeTimeSlots = AvailabilityCalculatorService.CalculateFreeSlots(availabilityList, allBookedIntervals);

            // 4. Map the resulting TimeSlots to FreeSlotDto
            var freeSlotDtos = freeTimeSlots.Select(slot =>
            {
                var parentAvailability = availabilityList.FirstOrDefault(a => slot.Start >= a.StartTime && slot.End <= a.EndTime);
                return new FreeSlotDto
                {
                    Id = parentAvailability?.Id ?? Guid.Empty,
                    CoachId = coachId,
                    StartTime = slot.Start,
                    EndTime = slot.End
                };
            }).ToList();

            // 5. Map the booking entities to BookedSlotDto
            var bookedSlotDtos = bookingEntities.Select(round => new BookedSlotDto
            {
                BookingId = round.BookingRequestId,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                CandidateName = round.BookingRequest?.Candidate?.User?.FullName ?? string.Empty,
                InterviewType = round.CoachInterviewService?.InterviewType?.Name
                    ?? (round.BookingRequest?.Type == Domain.Entities.Constants.BookingRequestType.Direct
                        ? "Direct Booking"
                        : "Interview"),
                Status = round.BookingRequest?.Status.ToString() ?? string.Empty
            }).ToList();

            // 6. Combine into the final DTO
            var schedule = new CoachScheduleDto
            {
                FreeSlots = freeSlotDtos,
                BookedSlots = bookedSlotDtos.OrderBy(b => b.StartTime).ToList()
            };

            return schedule;
        }

        public Task<CoachAvailability?> GetAsync(Guid coachId, DateTime startTime)
        {
            return _coachAvailabilitiesRepository.GetAsync(coachId, startTime);
        }
    }
}
