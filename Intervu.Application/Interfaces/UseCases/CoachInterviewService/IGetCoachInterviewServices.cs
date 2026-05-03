using Intervu.Application.DTOs.CoachInterviewService;

namespace Intervu.Application.Interfaces.UseCases.CoachInterviewService
{
    public interface IGetCoachInterviewServices
    {
        /// <param name="includeUnavailableForCoach">When true (coach dashboard), returns all services including those whose interview type is not Active. When false (public/candidate), returns only bookable services.</param>
        Task<IEnumerable<CoachInterviewServiceDto>> ExecuteAsync(Guid coachId, bool includeUnavailableForCoach = false);
    }
}
