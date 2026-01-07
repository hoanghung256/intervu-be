using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IUpdateAvailabilityStatus
    {
        Task<InterviewerAvailability> ExecuteAsync(Guid availabilityId, bool isBooked);
    }
}
