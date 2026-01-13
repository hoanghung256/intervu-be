using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IUpdateCoachAvailability
    {
        Task<bool> ExecuteAsync(Guid availabilityId, CoachAvailabilityUpdateDto dto);
    }
}
