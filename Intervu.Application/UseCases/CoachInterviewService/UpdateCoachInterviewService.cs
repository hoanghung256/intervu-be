using AutoMapper;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachInterviewService
{
    internal class UpdateCoachInterviewService : IUpdateCoachInterviewService
    {
        private readonly ICoachInterviewServiceRepository _serviceRepo;
        private readonly IInterviewTypeRepository _typeRepo;
        private readonly IMapper _mapper;

        public UpdateCoachInterviewService(
            ICoachInterviewServiceRepository serviceRepo,
            IInterviewTypeRepository typeRepo,
            IMapper mapper)
        {
            _serviceRepo = serviceRepo;
            _typeRepo = typeRepo;
            _mapper = mapper;
        }

        public async Task<CoachInterviewServiceDto> ExecuteAsync(Guid coachId, Guid serviceId, UpdateCoachInterviewServiceDto dto)
        {
            var service = await _serviceRepo.GetByIdWithDetailsAsync(serviceId)
                ?? throw new NotFoundException("Coach interview service not found");

            if (service.CoachId != coachId)
                throw new ForbiddenException("You can only update your own interview services");

            var interviewType = await _typeRepo.GetByIdAsync(service.InterviewTypeId)
                ?? throw new NotFoundException("Interview type not found");

            // Validate price within allowed range
            if (dto.Price < interviewType.MinPrice || dto.Price > interviewType.MaxPrice)
                throw new BadRequestException(
                    $"Price must be between {interviewType.MinPrice} and {interviewType.MaxPrice} for this interview type");

            service.Price = dto.Price;
            service.DurationMinutes = dto.DurationMinutes;

            _serviceRepo.UpdateAsync(service);
            await _serviceRepo.SaveChangesAsync();

            // Reload for mapping
            var updated = await _serviceRepo.GetByIdWithDetailsAsync(serviceId)
                ?? throw new NotFoundException("Failed to reload updated service");

            return _mapper.Map<CoachInterviewServiceDto>(updated);
        }
    }
}
