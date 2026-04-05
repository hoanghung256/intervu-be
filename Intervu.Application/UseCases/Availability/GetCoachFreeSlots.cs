using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Availability
{
    /// <summary>
    /// With the 30-min block model, each CoachAvailability record is an individual block.
    /// Free slots are simply all blocks with Status == Available.
    /// The frontend receives block IDs to use when booking.
    /// </summary>
    public class GetCoachFreeSlots : IGetCoachFreeSlots
    {
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;

        public GetCoachFreeSlots(ICoachAvailabilitiesRepository availabilityRepo)
        {
            _availabilityRepo = availabilityRepo;
        }

        public async Task<List<FreeSlotDto>> ExecuteAsync(Guid coachId, int month = 0, int year = 0)
        {
            var availabilities = (await _availabilityRepo
                .GetCoachAvailabilitiesByMonthAsync(coachId, month, year))
                .Where(a => a.Status == CoachAvailabilityStatus.Available)
                .OrderBy(a => a.StartTime)
                .ToList();

            return availabilities.Select(a => new FreeSlotDto
            {
                Id = a.Id,
                CoachId = a.CoachId,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = 0
            }).ToList();
        }
    }
}
