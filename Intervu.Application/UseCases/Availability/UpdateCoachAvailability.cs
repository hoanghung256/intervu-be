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
    public class UpdateCoachAvailability : IUpdateCoachAvailability
    {
        private readonly ICoachAvailabilitiesRepository _repo;

        public UpdateCoachAvailability(ICoachAvailabilitiesRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ExecuteAsync(CoachAvailabilityUpdateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var utcNow = DateTimeOffset.UtcNow;
            var origStart = dto.OriginalStartTime.UtcDateTime;
            var origEnd = dto.OriginalEndTime.UtcDateTime;
            var newStart = dto.NewStartTime.UtcDateTime;
            var newEnd = dto.NewEndTime.UtcDateTime;

            // Basic validation
            if (newEnd <= newStart)
                throw new ArgumentException("NewEndTime must be greater than NewStartTime");

            if (newStart <= utcNow.UtcDateTime || newEnd <= utcNow.UtcDateTime)
                throw new ArgumentException("Cannot update availability to a time in the past");

            var newDuration = newEnd - newStart;
            if (newDuration < TimeSpan.FromMinutes(30))
                throw new ArgumentException("Availability range must be at least 30 minutes");

            if (newDuration.TotalMinutes % 30 != 0)
                throw new ArgumentException("Availability range must be a multiple of 30 minutes");

            // Get existing blocks in the original range
            var existingBlocks = await _repo.GetBlocksInRangeAsync(dto.CoachId, origStart, origEnd);
            if (!existingBlocks.Any())
                throw new ArgumentException("No existing blocks found in the original range");

            // Compute the set of 30-min slots for old and new ranges
            var oldSlots = GenerateSlots(origStart, origEnd);
            var newSlots = GenerateSlots(newStart, newEnd);

            // Diff: slots to remove (in old but not in new), slots to add (in new but not in old)
            var slotsToRemove = oldSlots.Except(newSlots).ToList();
            var slotsToAdd = newSlots.Except(oldSlots).ToList();

            // Check if any block in the removed range is Booked
            var blocksToRemove = existingBlocks
                .Where(b => slotsToRemove.Any(s => s == b.StartTime))
                .ToList();

            if (blocksToRemove.Any(b => b.Status == CoachAvailabilityStatus.Booked))
                throw new ArgumentException("Cannot update: range contains booked sessions");

            // Check overlap for newly added slots (exclude existing blocks from this coach in old range)
            if (slotsToAdd.Any())
            {
                var addStart = slotsToAdd.Min();
                var addEnd = slotsToAdd.Max().AddMinutes(30);
                var existingIds = existingBlocks.Select(b => b.Id).ToList();

                // Check if any OTHER blocks overlap with the new slots
                foreach (var slotStart in slotsToAdd)
                {
                    var slotEnd = slotStart.AddMinutes(30);
                    var startOffset = new DateTimeOffset(slotStart, TimeSpan.Zero);
                    var endOffset = new DateTimeOffset(slotEnd, TimeSpan.Zero);

                    // Exclude all blocks in the original range from overlap check
                    bool isAvailable = await _repo.IsCoachAvailableAsync(dto.CoachId, startOffset, endOffset);
                    if (!isAvailable)
                    {
                        // Check if the overlap is only with blocks we already own in the original range
                        var overlappingOwn = existingBlocks.Any(b => b.StartTime == slotStart);
                        if (!overlappingOwn)
                            throw new ArgumentException($"New time range overlaps with an existing availability block at {slotStart:HH:mm}");
                    }
                }
            }

            // Execute: delete removed blocks, add new blocks
            if (blocksToRemove.Any())
            {
                await _repo.DeleteMultipleAsync(blocksToRemove.Select(b => b.Id).ToList());
            }

            if (slotsToAdd.Any())
            {
                // Filter out slots that already exist (overlap with kept blocks)
                var existingStartTimes = existingBlocks.Select(b => b.StartTime).ToHashSet();
                var trulyNewSlots = slotsToAdd.Where(s => !existingStartTimes.Contains(s)).ToList();

                if (trulyNewSlots.Any())
                {
                    var newBlocks = trulyNewSlots.Select(slotStart => new CoachAvailability
                    {
                        CoachId = dto.CoachId,
                        StartTime = slotStart,
                        EndTime = slotStart.AddMinutes(30),
                        Status = CoachAvailabilityStatus.Available
                    }).ToList();

                    await _repo.CreateMultipleCoachAvailabilitiesAsync(newBlocks);
                }
            }

            return true;
        }

        private static List<DateTime> GenerateSlots(DateTime start, DateTime end)
        {
            var slots = new List<DateTime>();
            var current = start;
            while (current < end)
            {
                slots.Add(current);
                current = current.AddMinutes(30);
            }
            return slots;
        }
    }
}
