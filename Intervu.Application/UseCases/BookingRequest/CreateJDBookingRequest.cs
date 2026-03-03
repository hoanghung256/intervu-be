using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
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
        private readonly IMapper _mapper;

        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(48);
        private static readonly TimeSpan MinGapBetweenRounds = TimeSpan.FromMinutes(15);

        public CreateJDBookingRequest(
            IBookingRequestRepository bookingRepo,
            ICoachInterviewServiceRepository serviceRepo,
            ICoachProfileRepository coachRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _coachRepo = coachRepo;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, CreateJDBookingRequestDto dto)
        {
            // Validate coach exists
            var coach = await _coachRepo.GetProfileByIdAsync(dto.CoachId)
                ?? throw new NotFoundException("Coach profile not found");

            // Validate at least 2 rounds
            if (dto.Rounds == null || dto.Rounds.Count < 2)
                throw new BadRequestException("At least 2 rounds are required for JD multi-round interviews");

            // Validate all CoachInterviewServices exist and belong to the coach
            var serviceIds = dto.Rounds.Select(r => r.CoachInterviewServiceId).Distinct().ToList();
            var services = (await _serviceRepo.GetByIdsAsync(serviceIds)).ToList();

            if (services.Count != serviceIds.Count)
                throw new NotFoundException("One or more coach interview services not found");

            var invalidServices = services.Where(s => s.CoachId != dto.CoachId).ToList();
            if (invalidServices.Count > 0)
                throw new BadRequestException("One or more selected services do not belong to the specified coach");

            var serviceMap = services.ToDictionary(s => s.Id);

            // Order rounds by start time and validate gaps
            var orderedRounds = dto.Rounds.OrderBy(r => r.StartTime).ToList();

            // Validate all round start times are in the future
            if (orderedRounds.Any(r => r.StartTime <= DateTime.UtcNow))
                throw new BadRequestException("All round start times must be in the future");

            // Validate gap between consecutive rounds
            for (int i = 1; i < orderedRounds.Count; i++)
            {
                var prevService = serviceMap[orderedRounds[i - 1].CoachInterviewServiceId];
                var prevEndTime = orderedRounds[i - 1].StartTime.AddMinutes(prevService.DurationMinutes);

                if (orderedRounds[i].StartTime < prevEndTime.Add(MinGapBetweenRounds))
                    throw new BadRequestException(
                        $"Round {i + 1} must start at least 15 minutes after round {i} ends. " +
                        $"Round {i} ends at {prevEndTime:g}, round {i + 1} starts at {orderedRounds[i].StartTime:g}");
            }

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
