using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CreateExternalBookingRequest : ICreateExternalBookingRequest
    {
        private readonly IBookingRequestRepository _bookingRepo;
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly ICoachProfileRepository _coachRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly IMapper _mapper;

        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(48);

        public CreateExternalBookingRequest(
            IBookingRequestRepository bookingRepo,
            ICoachInterviewServiceRepository serviceRepo,
            ICoachProfileRepository coachRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _coachRepo = coachRepo;
            _availabilityRepo = availabilityRepo;
            _mapper = mapper;
        }

        public async Task<BookingRequestDto> ExecuteAsync(Guid candidateId, CreateExternalBookingRequestDto dto)
        {
            // Validate coach exists
            var coach = await _coachRepo.GetProfileByIdAsync(dto.CoachId)
                ?? throw new NotFoundException("Coach profile not found");

            // Validate CoachInterviewService exists and belongs to this coach
            var service = await _serviceRepo.GetByIdWithDetailsAsync(dto.CoachInterviewServiceId)
                ?? throw new NotFoundException("Coach interview service not found");

            if (service.CoachId != dto.CoachId)
                throw new BadRequestException("The selected service does not belong to the specified coach");

            // Validate start time is in the future
            if (dto.RequestedStartTime <= DateTime.UtcNow)
                throw new BadRequestException("Requested start time must be in the future");

            // Validate that the booking fits within an available coach availability range
            var endTime = dto.RequestedStartTime.AddMinutes(service.DurationMinutes);
            var containingAvailability = await _availabilityRepo.FindContainingAvailabilityAsync(
                dto.CoachId, dto.RequestedStartTime, endTime);

            if (containingAvailability == null)
                throw new BadRequestException(
                    $"The requested time range ({dto.RequestedStartTime:g} - {endTime:g}) " +
                    "does not fall within any of the coach's available time slots");

            var bookingRequest = new Domain.Entities.BookingRequest
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                CoachId = dto.CoachId,
                Type = BookingRequestType.External,
                Status = BookingRequestStatus.Pending,
                CoachInterviewServiceId = dto.CoachInterviewServiceId,
                RequestedStartTime = dto.RequestedStartTime,
                AimLevel = dto.AimLevel,
                TotalAmount = service.Price,
                ExpiresAt = DateTime.UtcNow.Add(DefaultExpiration),
                CreatedAt = DateTime.UtcNow
            };

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
