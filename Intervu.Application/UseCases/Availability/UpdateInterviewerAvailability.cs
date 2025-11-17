using System;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;

namespace Intervu.Application.UseCases.Availability
{
    public class UpdateInterviewerAvailability : IUpdateInterviewerAvailability
    {
        private readonly IInterviewerAvailabilitiesRepository _repo;
        private readonly IMapper _mapper;

        public UpdateInterviewerAvailability(IInterviewerAvailabilitiesRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<bool> ExecuteAsync(int availabilityId, InterviewerAvailabilityUpdateDto dto)
        {
            // basic validation
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (availabilityId <= 0)
                throw new ArgumentException("ID must be greater than 0");
            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("EndTime must be greater than StartTime");

            // prevent updating to the past
            if (dto.EndTime <= DateTime.UtcNow)
                throw new ArgumentException("Cannot update availability to a time in the past");

            var updated = await _repo.UpdateInterviewerAvailabilityAsync(availabilityId, dto);
            if (!updated)
                throw new InvalidOperationException($"Availability with ID {availabilityId} not found or could not be updated");

            return true;
        }
    }
}
