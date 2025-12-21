using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IDeleteInterviewerAvailability
    {
        Task<bool> ExecuteAsync(Guid availabilityId);
    }
}
