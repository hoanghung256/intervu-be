using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Intervu.Application.UseCases.Availability
{
    public class CreateCoachAvailability : ICreateCoachAvailability
    {
        private readonly ICoachAvailabilitiesRepository _repo;
        private readonly ICoachProfileRepository _coachProfileRepo;
        private readonly IInterviewTypeRepository _interviewTypeRepo;
        private readonly IMapper _mapper;

        public CreateCoachAvailability(ICoachAvailabilitiesRepository repo, ICoachProfileRepository coachProfileRepo, IInterviewTypeRepository interviewTypeRepo, IMapper mapper)
        {
            _repo = repo;
            _coachProfileRepo = coachProfileRepo;
            _interviewTypeRepo = interviewTypeRepo;
            _mapper = mapper;
        }

        public async Task<Guid> ExecuteAsync(CoachAvailabilityCreateDto dto)
        {

            // basic validation
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Verify coach exists
            var coachProfile = await _coachProfileRepo.GetProfileByIdAsync(dto.CoachId);
            if (coachProfile == null)
                throw new InvalidOperationException($"Coach with ID {dto.CoachId} does not exist");
            
            // Prevent creating in the past - proper UTC comparison with DateTimeOffset
            //if (dto.StartTime.Minute != 0 || dto.StartTime.Second != 0)
            //    throw new ArgumentException("Start time must be on the hour (e.g., 09:00, 14:00)");
            // DateTimeOffset automatically handles timezone-aware comparison
            var utcNow = DateTimeOffset.UtcNow;

            // Split into 0.5-hour slots
            var slots = new List<CoachAvailability>();
            //int numSlots = (int)duration;
            var startTimeUtc = dto.StartTime.UtcDateTime;

            //Time gap between slots
            var slotGapDuration = TimeSpan.FromMinutes(15);
            var endTimeUtc = dto.EndTime.UtcDateTime;

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
                if (startTimeUtc <= utcNow.UtcDateTime || endTimeUtc <= utcNow.UtcDateTime)
                    throw new ArgumentException("Cannot create availability in the past");

                //if (dto.EndTime.Minute != 0 || dto.EndTime.Second != 0)
                //    throw new ArgumentException("End time must be on the hour (e.g., 09:00, 14:00)");


                if (dto.EndTime <= dto.StartTime) throw new ArgumentException("EndTime must be greater than StartTime");

                slotDuration = (dto.EndTime.UtcDateTime - dto.StartTime.UtcDateTime);

                if (slotDuration < TimeSpan.FromHours(0.5))
                    throw new ArgumentException("Availability must be at least 30 minutes");
            }


            // Calculate duration in hours (use UtcDateTime to get UTC DateTime)
            //var duration = (dto.EndTime.UtcDateTime - dto.StartTime.UtcDateTime).TotalHours;
            //if (duration < 0.5)
            //    throw new ArgumentException("Availability must be at least 30 minutes");

            // Split into 0.5-hour slots
            //var slots = new List<CoachAvailability>();
            //int numSlots = (int)duration;
            //var startTimeUtc = dto.StartTime.UtcDateTime;

            //Time gap between slots
            //var slotGapDuration = TimeSpan.FromMinutes(15);
            //var endTimeUtc = dto.EndTime.UtcDateTime;

            while (startTimeUtc.Add(slotDuration) <= endTimeUtc)
            {
                var slotStart = startTimeUtc;

                var slotEnd = slotStart.Add(slotDuration);

                // Check overlap for each slot
                var startOffset = new DateTimeOffset(slotStart, TimeSpan.Zero);
                var endOffset = new DateTimeOffset(slotEnd, TimeSpan.Zero);

                bool isAvailable = await _repo.IsCoachAvailableAsync(dto.CoachId, startOffset, endOffset);
                if (!isAvailable)
                    throw new InvalidOperationException($"Time slot {slotStart:HH:mm} - {slotEnd:HH:mm} conflicts with existing availability");

                slots.Add(new CoachAvailability
                {
                    CoachId = dto.CoachId,
                    Focus = dto.Focus,
                    TypeId = dto.TypeId,
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    Status = Domain.Entities.Constants.CoachAvailabilityStatus.Available
                });

                startTimeUtc = slotEnd.Add(slotGapDuration);
            }

            // Bulk insert all slots
            var id = await _repo.CreateMultipleCoachAvailabilitiesAsync(slots);
            return slots.First().Id;
        }
    }
}