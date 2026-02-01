using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    public class CreateCoachAvailability : ICreateCoachAvailability
    {
        private readonly ICoachAvailabilitiesRepository _repo;
        private readonly ICoachProfileRepository _coachProfileRepo;
        private readonly IMapper _mapper;

        public CreateCoachAvailability(ICoachAvailabilitiesRepository repo, ICoachProfileRepository coachProfileRepo, IMapper mapper)
        {
            _repo = repo;
            _coachProfileRepo = coachProfileRepo;
            _mapper = mapper;
        }

        public async Task<Guid> ExecuteAsync(CoachAvailabilityCreateDto dto)
        {
            // basic validation
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.EndTime <= dto.StartTime) throw new ArgumentException("EndTime must be greater than StartTime");

            // Verify coach exists
            var coachProfile = await _coachProfileRepo.GetProfileByIdAsync(dto.CoachId);
            if (coachProfile == null)
                throw new InvalidOperationException($"Coach with ID {dto.CoachId} does not exist");

            // Validate times are on the hour (minute = 0)
            if (dto.StartTime.Minute != 0 || dto.StartTime.Second != 0)
                throw new ArgumentException("Start time must be on the hour (e.g., 09:00, 14:00)");
            if (dto.EndTime.Minute != 0 || dto.EndTime.Second != 0)
                throw new ArgumentException("End time must be on the hour (e.g., 09:00, 14:00)");

            // Prevent creating in the past - proper UTC comparison with DateTimeOffset
            // DateTimeOffset automatically handles timezone-aware comparison
            var utcNow = DateTimeOffset.UtcNow;
            if (dto.EndTime <= utcNow)
                throw new ArgumentException("Cannot create availability in the past");

            // Calculate duration in hours (use UtcDateTime to get UTC DateTime)
            var duration = (dto.EndTime.UtcDateTime - dto.StartTime.UtcDateTime).TotalHours;
            if (duration < 1)
                throw new ArgumentException("Availability must be at least 1 hour");

            // Split into 1-hour slots
            var slots = new List<CoachAvailability>();
            int numSlots = (int)duration;
            var startTimeUtc = dto.StartTime.UtcDateTime;

            for (int i = 0; i < numSlots; i++)
            {
                var slotStart = startTimeUtc.AddHours(i);
                var slotEnd = slotStart.AddHours(1);

                // Check overlap for each slot
                var startOffset = new DateTimeOffset(slotStart, TimeSpan.Zero);
                var endOffset = new DateTimeOffset(slotEnd, TimeSpan.Zero);
                bool isAvailable = await _repo.IsCoachAvailableAsync(dto.CoachId, startOffset, endOffset);
                if (!isAvailable)
                    throw new InvalidOperationException($"Time slot {slotStart:HH:mm} - {slotEnd:HH:mm} conflicts with existing availability");

                slots.Add(new CoachAvailability
                {
                    CoachId = dto.CoachId,
                    TypeId = dto.TypeId,
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsBooked = false
                });
            }

            // Bulk insert all slots
            var id = await _repo.CreateMultipleCoachAvailabilitiesAsync(slots);
            return slots.First().Id;
        }
    }
}