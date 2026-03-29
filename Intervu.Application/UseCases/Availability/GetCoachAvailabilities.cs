using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRequestRepository _bookingRequestRepository;

        public GetCoachAvailabilities(
            ICoachAvailabilitiesRepository coachAvailabilitiesRepository,
            ITransactionRepository transactionRepository,
            IBookingRequestRepository bookingRequestRepository)
        {
            _coachAvailabilitiesRepository = coachAvailabilitiesRepository;
            _transactionRepository = transactionRepository;
            _bookingRequestRepository = bookingRequestRepository;
        }

        public async Task<CoachScheduleDto> ExecuteAsync(Guid coachId, int month = 0, int year = 0)
        {
            // 1. Get all availability windows for the coach in the given month
            var availabilities = await _coachAvailabilitiesRepository.GetCoachAvailabilitiesByMonthAsync(coachId, month, year);
            var availabilityList = availabilities.ToList();

            // 2. Get all confirmed bookings (from both direct booking and JD booking flows)
            var directBookings = await _transactionRepository.GetConfirmedBookingEntitiesForCoachAsync(coachId, month, year);
            var jdBookings = await _bookingRequestRepository.GetConfirmedBookingEntitiesForCoachAsync(coachId, month, year);

            var allBookedIntervals = directBookings
                .Select(b => (b.BookedStartTime!.Value, b.BookedStartTime!.Value.AddMinutes(b.BookedDurationMinutes!.Value)))
                .Concat(jdBookings.Select(b => (b.StartTime, b.EndTime)))
                .ToList();

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
            var bookedSlotDtos = new List<BookedSlotDto>();
            bookedSlotDtos.AddRange(directBookings.Select(b => new BookedSlotDto
            {
                BookingId = b.Id,
                StartTime = b.BookedStartTime!.Value,
                EndTime = b.BookedStartTime!.Value.AddMinutes(b.BookedDurationMinutes!.Value),
                CandidateName = b.User?.FullName ?? string.Empty,
                InterviewType = "Direct Booking", 
                Status = b.Status.ToString()
            }));

            bookedSlotDtos.AddRange(jdBookings.Select(b => new BookedSlotDto
            {
                // Link back to original booking request or room id depending on UI
                BookingId = b.BookingRequestId,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                CandidateName = b.BookingRequest.Candidate.User.FullName,
                InterviewType = b.BookingRequest.CoachInterviewService?.InterviewType?.Name ?? "JD Multi-Round Interview",
                Status = b.BookingRequest.Status.ToString()
            }));

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
