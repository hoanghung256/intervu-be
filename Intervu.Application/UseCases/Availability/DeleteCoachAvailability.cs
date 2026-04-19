using System;
using System.Linq;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    public class DeleteCoachAvailability : IDeleteCoachAvailability
    {
        private readonly ICoachAvailabilitiesRepository _repo;

        public DeleteCoachAvailability(ICoachAvailabilitiesRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Delete a single 30-min block by ID.
        /// </summary>
        public async Task<bool> ExecuteAsync(Guid availabilityId)
        {
            if (availabilityId == Guid.Empty)
                throw new BadRequestException("Availability ID must be a valid GUID");

            var availability = await _repo.GetByIdAsync(availabilityId);
            if (availability == null)
                throw new NotFoundException("Availability not found");

            if (availability.Status == CoachAvailabilityStatus.Booked)
                throw new ConflictException("Cannot delete a booked slot");

            if (availability.Status != CoachAvailabilityStatus.Available)
                throw new ConflictException("You can only delete available slots.");

            var deleted = await _repo.DeleteCoachAvailabilityAsync(availabilityId);
            if (!deleted)
                throw new NotFoundException($"Availability with ID {availabilityId} not found or could not be deleted");

            return true;
        }

        /// <summary>
        /// Delete all blocks within a specific time range for a coach.
        /// Refuses if any block in the range is Booked.
        /// </summary>
        public async Task<bool> ExecuteRangeAsync(CoachAvailabilityDeleteDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Request body is required");

            var rangeStart = dto.RangeStartTime.UtcDateTime;
            var rangeEnd = dto.RangeEndTime.UtcDateTime;

            if (rangeEnd <= rangeStart)
                throw new BadRequestException("RangeEndTime must be greater than RangeStartTime");

            var blocks = await _repo.GetBlocksInRangeAsync(dto.CoachId, rangeStart, rangeEnd);
            if (!blocks.Any())
                throw new NotFoundException("No availability blocks found in the specified range");

            if (blocks.Any(b => b.Status == CoachAvailabilityStatus.Booked))
                throw new ConflictException("Cannot delete: range contains booked sessions");

            if (blocks.Any(b => b.Status != CoachAvailabilityStatus.Available))
                throw new ConflictException("You can only delete available slots.");

            var ids = blocks.Select(b => b.Id).ToList();
            var deletedCount = await _repo.DeleteMultipleAsync(ids);

            return deletedCount > 0;
        }
    }
}
