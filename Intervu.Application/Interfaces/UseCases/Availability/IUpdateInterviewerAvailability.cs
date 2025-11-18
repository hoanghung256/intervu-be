using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface IUpdateInterviewerAvailability
    {
        Task<bool> ExecuteAsync(int availabilityId, InterviewerAvailabilityUpdateDto dto);
    }
}
