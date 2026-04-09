using AutoMapper;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using Intervu.Domain.Repositories;


namespace Intervu.Application.UseCases.CoachInterviewService
{
    internal class CreateCoachInterviewService : ICreateCoachInterviewService
    {
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly ICoachProfileRepository _coachRepo;
        private readonly IInterviewTypeRepository _typeRepo;
        private readonly IMapper _mapper;

        public CreateCoachInterviewService(
            ICoachInterviewServiceRepository serviceRepo,
            ICoachProfileRepository coachRepo,
            IInterviewTypeRepository typeRepo,
            IMapper mapper)
        {
            _serviceRepo = serviceRepo;
            _coachRepo = coachRepo;
            _typeRepo = typeRepo;
            _mapper = mapper;
        }

        public async Task<CoachInterviewServiceDto> ExecuteAsync(Guid coachId, CreateCoachInterviewServiceDto dto)
        {
            if (dto.DurationMinutes % 30 != 0)
            {
                throw new BadRequestException("Duration must be a multiple of 30 minutes.");
            }

            var coach = await _coachRepo.GetProfileByIdAsync(coachId)
                ?? throw new NotFoundException("Coach profile not found");

            var interviewType = await _typeRepo.GetByIdAsync(dto.InterviewTypeId)
                ?? throw new NotFoundException("Interview type not found");

            // Validate price within allowed range
            if (dto.Price < interviewType.MinPrice || dto.Price > interviewType.MaxPrice)
                throw new BadRequestException(
                    $"Price must be between {interviewType.MinPrice} and {interviewType.MaxPrice} for this interview type");

            // Check for duplicate (coach already offers this type)
            var existing = await _serviceRepo.GetByCoachAndTypeAsync(coachId, dto.InterviewTypeId);
            if (existing != null)
                throw new ConflictException("Coach already offers this interview type. Update the existing service instead.");

            var service = _mapper.Map<Domain.Entities.CoachInterviewService>(dto);
            service.CoachId = coachId;

            await _serviceRepo.AddAsync(service);
            await _serviceRepo.SaveChangesAsync();

            // Reload with nav props for DTO
            var created = await _serviceRepo.GetByIdWithDetailsAsync(service.Id)
                ?? throw new NotFoundException("Failed to reload created service");

            return _mapper.Map<CoachInterviewServiceDto>(created);
        }
    }
}
