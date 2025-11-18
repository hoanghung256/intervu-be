using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;

namespace Intervu.Application.UseCases.Availability
{
    public class CreateInterviewerAvailability : ICreateInterviewerAvailability
    {
        private readonly IInterviewerAvailabilitiesRepository _repo;
        private readonly IMapper _mapper;

        public CreateInterviewerAvailability(IInterviewerAvailabilitiesRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<int> ExecuteAsync(InterviewerAvailabilityCreateDto dto)
        {
            // basic validation
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.EndTime <= dto.StartTime) throw new ArgumentException("EndTime must be greater than StartTime");

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
            var slots = new List<InterviewerAvailability>();
            int numSlots = (int)duration;
            var startTimeUtc = dto.StartTime.UtcDateTime;

            for (int i = 0; i < numSlots; i++)
            {
                var slotStart = startTimeUtc.AddHours(i);
                var slotEnd = slotStart.AddHours(1);

                // Check overlap for each slot
                var startOffset = new DateTimeOffset(slotStart, TimeSpan.Zero);
                var endOffset = new DateTimeOffset(slotEnd, TimeSpan.Zero);
                bool isAvailable = await _repo.IsInterviewerAvailableAsync(dto.InterviewerId, startOffset, endOffset);
                if (!isAvailable)
                    throw new InvalidOperationException($"Time slot {slotStart:HH:mm} - {slotEnd:HH:mm} conflicts with existing availability");

                slots.Add(new InterviewerAvailability
                {
                    InterviewerId = dto.InterviewerId,
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsBooked = false
                });
            }

            // Bulk insert all slots
            var id = await _repo.CreateMultipleInterviewerAvailabilitiesAsync(slots);
            return id;
        }
    }
}