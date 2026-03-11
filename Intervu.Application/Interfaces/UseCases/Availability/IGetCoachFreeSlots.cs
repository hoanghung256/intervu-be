using Intervu.Application.DTOs.Availability;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    /// <summary>
    /// Returns computed free time slots for a coach (availability minus active bookings).
    /// Used by the candidate booking flow.
    /// </summary>
    public interface IGetCoachFreeSlots
    {
        Task<List<FreeSlotDto>> ExecuteAsync(Guid coachId, int month = 0, int year = 0);
    }
}
