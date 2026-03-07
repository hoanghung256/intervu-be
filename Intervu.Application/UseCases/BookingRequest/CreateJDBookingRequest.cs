using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Services;
using Intervu.Application.Validators;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CreateJDBookingRequest : ICreateJDBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly ICoachProfileRepository _coachRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IMapper _mapper;

        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(48);

        public CreateJDBookingRequest(
            IBookingRequestRepository bookingRepo,
            ICoachInterviewServiceRepository serviceRepo,
            ICoachProfileRepository coachRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            ITransactionRepository transactionRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _coachRepo = coachRepo;
            _availabilityRepo = availabilityRepo;
            _transactionRepo = transactionRepo;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, CreateJDBookingRequestDto dto)
        {
            // Validate coach exists
            var coach = await _coachRepo.GetProfileByIdAsync(dto.CoachId)
                ?? throw new NotFoundException("Coach profile not found");

            // Validate all CoachInterviewServices exist and belong to the coach
            var serviceIds = dto.Rounds.Select(r => r.CoachInterviewServiceId).Distinct().ToList();
            var services = (await _serviceRepo.GetByIdsAsync(serviceIds)).ToList();

            if (services.Count != serviceIds.Count)
                throw new NotFoundException("One or more coach interview services not found");

            var invalidServices = services.Where(s => s.CoachId != dto.CoachId).ToList();
            if (invalidServices.Count > 0)
                throw new BadRequestException("One or more selected services do not belong to the specified coach");

            var serviceMap = services.ToDictionary(s => s.Id);
            var serviceDurations = services.ToDictionary(s => s.Id, s => s.DurationMinutes);

            // ── Subtraction pattern: compute free slots ──────────────
            // Fetch ALL raw availability windows (month=0, year=0) for this coach
            var rawAvailabilities = (await _availabilityRepo
                .GetCoachAvailabilitiesByMonthAsync(dto.CoachId, 0, 0))
                .ToList();

            if (rawAvailabilities.Count == 0)
                throw new BadRequestException("This coach has no available time slots");

            // Determine range covered by rounds to fetch only relevant bookings
            var orderedRounds = dto.Rounds.OrderBy(r => r.StartTime).ToList();
            var rangeStart = rawAvailabilities.Min(a => a.StartTime);
            var rangeEnd = rawAvailabilities.Max(a => a.EndTime);

            var activeBookings = await _transactionRepo
                .GetActiveBookingsByCoachAsync(dto.CoachId, rangeStart, rangeEnd);

            var freeTimeSlots = AvailabilityCalculatorService
                .CalculateFreeSlots(rawAvailabilities, activeBookings);

            // Map to FreeSlotDto for the validator
            var freeSlotDtos = freeTimeSlots.Select(slot =>
            {
                var parent = rawAvailabilities.FirstOrDefault(a =>
                    a.StartTime <= slot.Start && a.EndTime >= slot.End);
                return new FreeSlotDto
                {
                    Id = parent?.Id ?? Guid.Empty,
                    CoachId = dto.CoachId,
                    StartTime = slot.Start,
                    EndTime = slot.End,
                    Status = 0
                };
            }).ToList();

            // ── Validate all business rules via static validator ─────
            MultiRoundBookingValidator.ValidateMultiRoundBooking(dto, freeSlotDtos, serviceDurations);

            // Calculate total price from all rounds
            var totalAmount = 0;
            var rounds = new List<InterviewRound>();

            for (int i = 0; i < orderedRounds.Count; i++)
            {
                var roundDto = orderedRounds[i];
                var service = serviceMap[roundDto.CoachInterviewServiceId];

                var round = new InterviewRound
                {
                    Id = Guid.NewGuid(),
                    CoachInterviewServiceId = roundDto.CoachInterviewServiceId,
                    RoundNumber = i + 1,
                    StartTime = roundDto.StartTime,
                    EndTime = roundDto.StartTime.AddMinutes(service.DurationMinutes),
                    Price = service.Price
                };

                rounds.Add(round);
                totalAmount += service.Price;
            }

            var bookingRequest = new Domain.Entities.BookingRequest
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                CoachId = dto.CoachId,
                Type = BookingRequestType.JDInterview,
                Status = BookingRequestStatus.Pending,
                JobDescriptionUrl = dto.JobDescriptionUrl,
                CVUrl = dto.CVUrl,
                AimLevel = dto.AimLevel,
                TotalAmount = totalAmount,
                ExpiresAt = DateTime.UtcNow.Add(DefaultExpiration),
                CreatedAt = DateTime.UtcNow
            };

            // Link rounds to the booking request
            foreach (var round in rounds)
            {
                round.BookingRequestId = bookingRequest.Id;
                bookingRequest.Rounds.Add(round);
            }

            await _bookingRepo.AddAsync(bookingRequest);
            await _bookingRepo.SaveChangesAsync();

            // Reload with navigation properties
            var created = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequest.Id)
                ?? throw new NotFoundException("Failed to reload created booking request");

            var result = _mapper.Map<BookingRequestDto>(created);
            result.CandidateName = created.Candidate?.User?.FullName;
            result.CoachName = created.Coach?.User?.FullName;

            return result;
        }
    }
}
