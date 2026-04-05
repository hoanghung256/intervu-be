using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Exceptions;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Validators
{
    public static class MultiRoundBookingValidator
    {
        private const int AvailabilityBlockMinutes = 30;

        /// <summary>
        /// Validates a JD booking request against the block-based availability model.
        /// For each round:
        ///   1. Consecutive check: blocks must be strictly consecutive (Block[n].EndTime == Block[n+1].StartTime)
        ///   2. Status check: all blocks must be Available
        ///   3. Service alignment: block count must match ceil(Service.DurationPerRound / 30)
        /// </summary>
        public static void ValidateMultiRoundBooking(
            CreateJDBookingRequestDto request,
            Dictionary<Guid, CoachAvailability> blockLookup,
            Dictionary<Guid, int> serviceDurations)
        {
            if (request.Rounds == null || request.Rounds.Count < 1)
                throw new BadRequestException("At least 1 round is required");

            for (int i = 0; i < request.Rounds.Count; i++)
            {
                var roundDto = request.Rounds[i];
                var roundLabel = $"Round {i + 1}";

                if (roundDto.AvailabilityIds == null || roundDto.AvailabilityIds.Count == 0)
                    throw new BadRequestException($"{roundLabel}: at least one availability block is required");

                // Resolve all blocks for this round
                var blocks = new List<CoachAvailability>();
                foreach (var blockId in roundDto.AvailabilityIds)
                {
                    if (!blockLookup.TryGetValue(blockId, out var block))
                        throw new BadRequestException($"{roundLabel}: availability block {blockId} not found");
                    blocks.Add(block);
                }

                // Status check: all blocks must be Available
                var bookedBlock = blocks.FirstOrDefault(b => b.Status != CoachAvailabilityStatus.Available);
                if (bookedBlock != null)
                    throw new BadRequestException(
                        $"{roundLabel}: block {bookedBlock.Id} ({bookedBlock.StartTime:HH:mm}-{bookedBlock.EndTime:HH:mm}) is not available (status: {bookedBlock.Status})");

                // Service alignment check: block count must match ceil(duration / 30)
                if (!serviceDurations.TryGetValue(roundDto.CoachInterviewServiceId, out var durationMinutes))
                    throw new BadRequestException(
                        $"{roundLabel}: service {roundDto.CoachInterviewServiceId} not found");

                var expectedBlockCount = (durationMinutes + (AvailabilityBlockMinutes - 1)) / AvailabilityBlockMinutes;
                if (blocks.Count != expectedBlockCount)
                    throw new BadRequestException(
                        $"{roundLabel}: expected {expectedBlockCount} blocks for {durationMinutes}-minute service, but got {blocks.Count}");

                // Consecutive check: sort by StartTime, then verify strict adjacency
                var sortedBlocks = blocks.OrderBy(b => b.StartTime).ToList();
                for (int j = 1; j < sortedBlocks.Count; j++)
                {
                    if (sortedBlocks[j].StartTime != sortedBlocks[j - 1].EndTime)
                        throw new BadRequestException(
                            $"{roundLabel}: blocks are not consecutive. " +
                            $"Block ending at {sortedBlocks[j - 1].EndTime:HH:mm} is not adjacent to block starting at {sortedBlocks[j].StartTime:HH:mm}");
                }

                // Verify all blocks belong to the same coach
                var coachIds = blocks.Select(b => b.CoachId).Distinct().ToList();
                if (coachIds.Count > 1)
                    throw new BadRequestException($"{roundLabel}: blocks belong to different coaches");

                if (coachIds[0] != request.CoachId)
                    throw new BadRequestException($"{roundLabel}: blocks do not belong to the specified coach");

                // Verify start time is in the future
                if (sortedBlocks[0].StartTime <= DateTime.UtcNow)
                    throw new BadRequestException($"{roundLabel}: start time must be in the future");
            }

            // Cross-round validation: no block used in multiple rounds
            var allBlockIds = request.Rounds.SelectMany(r => r.AvailabilityIds).ToList();
            var duplicates = allBlockIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
                throw new BadRequestException(
                    $"Availability blocks used in multiple rounds: {string.Join(", ", duplicates)}");
        }
    }
}
