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
        private readonly IInterviewTypeRepository _interviewTypeRepo;
        private readonly IMapper _mapper;

        public UpdateCoachAvailability(ICoachAvailabilitiesRepository repo, IInterviewTypeRepository interviewTypeRepo, IMapper mapper)
        {
            _repo = repo;
            _interviewTypeRepo = interviewTypeRepo;
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
                throw new ArgumentException("You can only update available slots.");

            var utcNow = DateTimeOffset.UtcNow;

            TimeSpan slotDuration;

            if (dto.Focus == InterviewFocus.General_Skills)
            {
                if (!dto.TypeId.HasValue)
                    throw new ArgumentException("TypeId is required for General Skill interview");

                var type = await _interviewTypeRepo.GetByIdAsync(dto.TypeId.Value);
                if (type == null)
                    throw new ArgumentException("Interview type not found");

                slotDuration = TimeSpan.FromMinutes(type.DurationMinutes);
            }
            else // JD
            {

                if (dto.EndTime <= dto.StartTime) throw new ArgumentException("EndTime must be greater than StartTime");

                if (dto.EndTime <= utcNow || dto.StartTime <= utcNow)
                    throw new ArgumentException("Cannot update availability to a time in the past");

                slotDuration = (dto.EndTime.UtcDateTime - dto.StartTime.UtcDateTime);

                if (slotDuration < TimeSpan.FromHours(0.5))
                    throw new ArgumentException("Availability must be at least 30 minutes");
            }

            dto.EndTime = dto.StartTime.Add(slotDuration);

            var startOffset = new DateTimeOffset(dto.StartTime.UtcDateTime, TimeSpan.Zero);
            var endOffset = new DateTimeOffset(dto.EndTime.UtcDateTime, TimeSpan.Zero);

            var minGap = TimeSpan.FromMinutes(15);

            var bufferStart = startOffset.Add(-minGap);
            var bufferEnd = endOffset.Add(minGap);

            bool isAvailable = await _repo.IsCoachAvailableAsync(dto.CoachId, bufferStart, bufferEnd, availabilityId);
            if (!isAvailable)
                throw new ArgumentException($"Time slot gap is within {minGap.TotalMinutes} minutes");

            var updated = await _repo.UpdateCoachAvailabilityAsync(
                availabilityId,
                dto.Focus,
                dto.StartTime,
                dto.EndTime,
                dto.TypeId
            );

            if (!updated)
                throw new ArgumentException($"Availability with ID {availabilityId} not found or could not be updated");

            return true;
        }
    }
}