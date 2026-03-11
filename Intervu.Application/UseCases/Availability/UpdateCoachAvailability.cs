using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Availability
{
    public class UpdateCoachAvailability : IUpdateCoachAvailability
    {
        private readonly ICoachAvailabilitiesRepository _repo;
        private readonly IMapper _mapper;

        public UpdateCoachAvailability(ICoachAvailabilitiesRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<bool> ExecuteAsync(Guid availabilityId, CoachAvailabilityUpdateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var availability = await _repo.GetByIdAsync(availabilityId);
            if (availability == null)
                throw new ArgumentException("Availability not found");

            if (availability.Status != CoachAvailabilityStatus.Available)
                throw new ArgumentException("You can only update available time ranges.");

            var utcNow = DateTimeOffset.UtcNow;

            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("EndTime must be greater than StartTime");

            if (dto.EndTime <= utcNow || dto.StartTime <= utcNow)
                throw new ArgumentException("Cannot update availability to a time in the past");

            var duration = dto.EndTime.UtcDateTime - dto.StartTime.UtcDateTime;
            if (duration < TimeSpan.FromMinutes(30))
                throw new ArgumentException("Availability must be at least 30 minutes");

            // Check overlap with minimum 15-minute gap
            var startOffset = new DateTimeOffset(dto.StartTime.UtcDateTime, TimeSpan.Zero);
            var endOffset = new DateTimeOffset(dto.EndTime.UtcDateTime, TimeSpan.Zero);
            var minGap = TimeSpan.FromMinutes(15);
            var bufferStart = startOffset.Add(-minGap);
            var bufferEnd = endOffset.Add(minGap);

            bool isAvailable = await _repo.IsCoachAvailableAsync(dto.CoachId, bufferStart, bufferEnd, availabilityId);
            if (!isAvailable)
                throw new ArgumentException($"Time range overlaps or is within {minGap.TotalMinutes} minutes of an existing availability");

            var updated = await _repo.UpdateCoachAvailabilityAsync(
                availabilityId,
                dto.StartTime,
                dto.EndTime
            );

            if (!updated)
                throw new ArgumentException($"Availability with ID {availabilityId} not found or could not be updated");

            return true;
        }
    }
}