using System;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

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

            // Validate times are on the hour (minute = 0)
            if (dto.StartTime.Minute != 0 || dto.StartTime.Second != 0)
                throw new ArgumentException("Start time must be on the hour (e.g., 09:00, 14:00)");
            if (dto.EndTime.Minute != 0 || dto.EndTime.Second != 0)
                throw new ArgumentException("End time must be on the hour (e.g., 09:00, 14:00)");

            // Prevent updating to the past - proper UTC comparison with DateTimeOffset
            var utcNow = DateTimeOffset.UtcNow;
            if (dto.EndTime <= utcNow)
                throw new ArgumentException("Cannot update availability to a time in the past");

            var updated = await _repo.UpdateInterviewerAvailabilityAsync(availabilityId, dto.StartTime, dto.EndTime);
            if (!updated)
                throw new InvalidOperationException($"Availability with ID {availabilityId} not found or could not be updated");

            return true;
        }
    }
}
