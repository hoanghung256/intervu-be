using Intervu.Application.DTOs.CoachInterviewService;

namespace Intervu.Application.Interfaces.UseCases.CoachInterviewService
{
    public interface IGetCoachInterviewServices
    {
        Task<IEnumerable<CoachInterviewServiceDto>> ExecuteAsync(Guid coachId);
    }
}
