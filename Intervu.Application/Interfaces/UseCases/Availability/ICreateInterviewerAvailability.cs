using System.Threading.Tasks;
using Intervu.Application.DTOs.Availability;

namespace Intervu.Application.Interfaces.UseCases.Availability
{
    public interface ICreateInterviewerAvailability
    {
        Task<Guid> ExecuteAsync(InterviewerAvailabilityCreateDto dto);
    }
}