using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IDeleteCoachAvailability
    {
        Task<bool> ExecuteAsync(Guid availabilityId);
        Task<bool> ExecuteRangeAsync(CoachAvailabilityDeleteDto dto);
    }
}
