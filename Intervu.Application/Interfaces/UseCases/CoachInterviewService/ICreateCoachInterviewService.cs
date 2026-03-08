using Intervu.Application.DTOs.CoachInterviewService;

namespace Intervu.Application.Interfaces.UseCases.CoachInterviewService
{
    public interface ICreateCoachInterviewService
    {
        Task<CoachInterviewServiceDto> ExecuteAsync(Guid coachId, CreateCoachInterviewServiceDto dto);
    }
}
