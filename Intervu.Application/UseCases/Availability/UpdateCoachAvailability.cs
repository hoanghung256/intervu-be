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
            // basic validation
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var availability = await _repo.GetByIdAsync(availabilityId);
            if (availability == null)
                throw new InvalidOperationException("Availability not found");

            if (availability.Status != CoachAvailabilityStatus.Available)
                throw new ArgumentException("You can only update available slots.");

            // Validate times are on the hour (minute = 0)
            //if (dto.StartTime.Minute != 0 || dto.StartTime.Second != 0)
            //    throw new ArgumentException("Start time must be on the hour (e.g., 09:00, 14:00)");

            var utcNow = DateTimeOffset.UtcNow;

            var slotGap = TimeSpan.FromMinutes(15);

            TimeSpan slotDuration;

            if (dto.Focus == InterviewFocus.General_Skills)
            {
                if (!dto.TypeId.HasValue)
                    throw new ArgumentException("TypeId is required for General Skill interview");

                var type = await _interviewTypeRepo.GetByIdAsync(dto.TypeId.Value);
                if (type == null)
                    throw new InvalidOperationException("Interview type not found");

                slotDuration = TimeSpan.FromMinutes(type.DurationMinutes);
            }
            else // JD
            {

                if (dto.EndTime <= dto.StartTime) throw new ArgumentException("EndTime must be greater than StartTime");

                //if (dto.EndTime.Minute != 0 || dto.EndTime.Second != 0)
                //    throw new ArgumentException("End time must be on the hour (e.g., 09:00, 14:00)");

                if (dto.EndTime <= utcNow || dto.StartTime <= utcNow)
                    throw new ArgumentException("Cannot update availability to a time in the past");

                slotDuration = (dto.EndTime.UtcDateTime - dto.StartTime.UtcDateTime);

                if (slotDuration < TimeSpan.FromHours(0.5))
                    throw new ArgumentException("Availability must be at least 30 minutes");
            }

            dto.EndTime = dto.StartTime.Add(slotDuration);

            var checkStart = dto.StartTime.Subtract(slotGap);
            var checkEnd = dto.EndTime.Add(slotGap);

            bool isValidSlotGap = await _repo.IsCoachAvailableAsync(availability.CoachId, checkStart, checkEnd);

            var updated = await _repo.UpdateCoachAvailabilityAsync(
                availabilityId,
                dto.Focus,
                dto.StartTime,
                dto.EndTime,
                dto.TypeId
            );

            if (!updated)
                throw new InvalidOperationException($"Availability with ID {availabilityId} not found or could not be updated");

            return true;
        }
    }
}