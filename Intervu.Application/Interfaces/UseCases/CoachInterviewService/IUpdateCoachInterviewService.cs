using Intervu.Application.DTOs.CoachInterviewService;

namespace Intervu.Application.Interfaces.UseCases.CoachInterviewService
{
    public interface IUpdateCoachInterviewService
    {
        Task<CoachInterviewServiceDto> ExecuteAsync(Guid coachId, Guid serviceId, UpdateCoachInterviewServiceDto dto);
    }
}
