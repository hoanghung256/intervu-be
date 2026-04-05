using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface ICreateCoachAvailability
    {
        Task<List<Guid>> ExecuteAsync(CoachAvailabilityCreateDto dto);
    }
}
