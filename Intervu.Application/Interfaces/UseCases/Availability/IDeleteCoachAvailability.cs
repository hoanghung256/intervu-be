using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IDeleteCoachAvailability
    {
        Task<bool> ExecuteAsync(Guid availabilityId);
    }
}
