using AutoMapper;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Application.Validators;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.BookingRequest
{
    internal class CreateJDBookingRequest : ICreateJDBookingRequest
    {
        private const int AvailabilityBlockMinutes = 30;

        private readonly IBookingRequestRepository _bookingRepo;
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly ICoachProfileRepository _coachRepo;
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBackgroundService _backgroundService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(48);

        public CreateJDBookingRequest(
            IBookingRequestRepository bookingRepo,
            ICoachInterviewServiceRepository serviceRepo,
            ICoachProfileRepository coachRepo,
            ICoachAvailabilitiesRepository availabilityRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBackgroundService backgroundService,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _coachRepo = coachRepo;
            _availabilityRepo = availabilityRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _backgroundService = backgroundService;
            _userRepository = userRepository;
            _configuration = configuration;
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

            // Begin atomic transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Collect all referenced availability block IDs across all rounds
                var allBlockIds = dto.Rounds.SelectMany(r => r.AvailabilityIds).Distinct().ToList();

                // Load and lock all blocks in the current transaction to prevent race conditions.
                var blockEntities = new Dictionary<Guid, CoachAvailability>();
                foreach (var blockId in allBlockIds)
                {
                    var block = await _availabilityRepo.GetByIdForUpdateAsync(blockId);
                    if (block == null)
                        throw new BadRequestException($"Availability block {blockId} not found");

                    blockEntities[blockId] = block;
                }

                // Validate while holding row locks so booking checks are transactionally consistent.
                MultiRoundBookingValidator.ValidateMultiRoundBooking(dto, blockEntities, serviceDurations);

                // Calculate total price and build rounds
                var totalAmount = 0;
                var rounds = new List<InterviewRound>();

                for (int i = 0; i < dto.Rounds.Count; i++)
                {
                    var roundDto = dto.Rounds[i];
                    var service = serviceMap[roundDto.CoachInterviewServiceId];
                    var requiredBlockCount = (service.DurationMinutes + (AvailabilityBlockMinutes - 1)) / AvailabilityBlockMinutes;

                    // Resolve blocks for this round, sorted by StartTime
                    var roundBlocks = roundDto.AvailabilityIds
                        .Select(id => blockEntities[id])
                        .OrderBy(b => b.StartTime)
                        .Take(requiredBlockCount)
                        .ToList();

                    if (roundBlocks.Count != requiredBlockCount)
                        throw new BadRequestException(
                            $"Round {i + 1}: expected {requiredBlockCount} blocks for {service.DurationMinutes}-minute service, but got {roundBlocks.Count}");

                    var roundStartTime = roundBlocks.First().StartTime;
                    var roundEndTime = roundStartTime.AddMinutes(service.DurationMinutes);

                    var round = new InterviewRound
                    {
                        Id = Guid.NewGuid(),
                        CoachInterviewServiceId = roundDto.CoachInterviewServiceId,
                        RoundNumber = i + 1,
                        StartTime = roundStartTime,
                        EndTime = roundEndTime,
                        Price = service.Price
                    };

                    rounds.Add(round);
                    totalAmount += service.Price;

                    // Mark all blocks as Booked and link to this round
                    foreach (var block in roundBlocks)
                    {
                        block.Status = CoachAvailabilityStatus.Booked;
                        block.InterviewRoundId = round.Id;
                        _availabilityRepo.UpdateAsync(block);
                    }
                }

                var bookingRequest = new Domain.Entities.BookingRequest
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidateId,
                    CoachId = dto.CoachId,
                    Type = BookingRequestType.JDInterview,
                    Status = BookingRequestStatus.Accepted,
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
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var candidateUser = await _userRepository.GetByIdAsync(candidateId);
                var coachUser = await _userRepository.GetByIdAsync(dto.CoachId);
                if (coachUser != null)
                {
                    var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
                    var placeholders = new Dictionary<string, string>
                    {
                        ["CoachName"] = coachUser.FullName,
                        ["CandidateName"] = candidateUser?.FullName ?? "Candidate",
                        ["TotalAmount"] = totalAmount.ToString("N0"),
                        ["RoundCount"] = rounds.Count.ToString(),
                        ["DashboardLink"] = $"{frontendUrl.TrimEnd('/')}/dashboard/booking-requests"
                    };

                    _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                        coachUser.Email,
                        "NewBookingRequest",
                        placeholders));
                }

                // Reload with navigation properties
                var created = await _bookingRepo.GetByIdWithDetailsAsync(bookingRequest.Id)
                    ?? throw new NotFoundException("Failed to reload created booking request");

                var result = _mapper.Map<BookingRequestDto>(created);
                result.CandidateName = created.Candidate?.User?.FullName;
                result.CoachName = created.Coach?.User?.FullName;

                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
