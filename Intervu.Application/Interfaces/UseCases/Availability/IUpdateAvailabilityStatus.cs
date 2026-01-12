using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IUpdateAvailabilityStatus
    {
        Task<CoachAvailability> ExecuteAsync(Guid availabilityId, bool isBooked);
    }
}
