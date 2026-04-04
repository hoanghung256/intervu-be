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

        public CreateCoachAvailability(ICoachAvailabilitiesRepository repo, ICoachProfileRepository coachProfileRepo)
        {
            _repo = repo;
            _coachProfileRepo = coachProfileRepo;
        }

        public async Task<List<Guid>> ExecuteAsync(CoachAvailabilityCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var coachProfile = await _coachProfileRepo.GetProfileByIdAsync(dto.CoachId);
            if (coachProfile == null)
                throw new ArgumentException($"Coach with ID {dto.CoachId} does not exist");

            var utcNow = DateTimeOffset.UtcNow;
            var rangeStart = dto.RangeStartTime.UtcDateTime;
            var rangeEnd = dto.RangeEndTime.UtcDateTime;

            if (rangeStart <= utcNow.UtcDateTime || rangeEnd <= utcNow.UtcDateTime)
                throw new ArgumentException("Cannot create availability in the past");

            if (rangeEnd <= rangeStart)
                throw new ArgumentException("RangeEndTime must be greater than RangeStartTime");

            var duration = rangeEnd - rangeStart;
            if (duration < TimeSpan.FromMinutes(30))
                throw new ArgumentException("Availability range must be at least 30 minutes");

            if (duration.TotalMinutes % 30 != 0)
                throw new ArgumentException("Availability range must be a multiple of 30 minutes");

            // Check overlap with ANY existing block in this range
            bool isAvailable = await _repo.IsCoachAvailableAsync(
                dto.CoachId,
                new DateTimeOffset(rangeStart, TimeSpan.Zero),
                new DateTimeOffset(rangeEnd, TimeSpan.Zero));

            if (!isAvailable)
                throw new ArgumentException("Time range overlaps with an existing availability block");

            // Split range into 30-minute blocks
            var blocks = new List<CoachAvailability>();
            var blockStart = rangeStart;

            while (blockStart < rangeEnd)
            {
                var blockEnd = blockStart.AddMinutes(30);
                blocks.Add(new CoachAvailability
                {
                    CoachId = dto.CoachId,
                    StartTime = blockStart,
                    EndTime = blockEnd,
                    Status = CoachAvailabilityStatus.Available
                });
                blockStart = blockEnd;
            }

            await _repo.CreateMultipleCoachAvailabilitiesAsync(blocks);
            return blocks.Select(b => b.Id).ToList();
        }
    }
}
