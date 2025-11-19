using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IUpdateAvailabilityStatus
    {
        Task<InterviewerAvailability> ExecuteAsync(int availabilityId, bool isBooked);
    }
}
