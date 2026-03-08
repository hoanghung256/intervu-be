using AutoMapper;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            // Verify coach exists
            var coachProfile = await _coachProfileRepo.GetProfileByIdAsync(dto.CoachId);
            if (coachProfile == null)
                throw new ArgumentException($"Coach with ID {dto.CoachId} does not exist");

            var utcNow = DateTimeOffset.UtcNow;
            var startTimeUtc = dto.StartTime.UtcDateTime;
            var endTimeUtc = dto.EndTime.UtcDateTime;

            if (startTimeUtc <= utcNow.UtcDateTime || endTimeUtc <= utcNow.UtcDateTime)
                throw new ArgumentException("Cannot create availability in the past");

            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("EndTime must be greater than StartTime");

            var duration = endTimeUtc - startTimeUtc;
            if (duration < TimeSpan.FromMinutes(30))
                throw new ArgumentException("Availability must be at least 30 minutes");

            // Check overlap with minimum 15-minute gap
            var minGap = TimeSpan.FromMinutes(15);
            var startOffset = new DateTimeOffset(startTimeUtc, TimeSpan.Zero);
            var endOffset = new DateTimeOffset(endTimeUtc, TimeSpan.Zero);
            var bufferStart = startOffset.Add(-minGap);
            var bufferEnd = endOffset.Add(minGap);

            bool isAvailable = await _repo.IsCoachAvailableAsync(dto.CoachId, bufferStart, bufferEnd);
            if (!isAvailable)
                throw new ArgumentException($"Time range overlaps or is within {minGap.TotalMinutes} minutes of an existing availability");

            var availability = new CoachAvailability
            {
                CoachId = dto.CoachId,
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                Status = CoachAvailabilityStatus.Available
            };

            var id = await _repo.CreateCoachAvailabilityAsync(availability);
            return id;
        }
    }
}