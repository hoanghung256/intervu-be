using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IUpdateAvailabilityStatus
    {
        Task<CoachAvailability> ExecuteAsync(Guid availabilityId, CoachAvailabilityStatus status);
    }
}
