using Intervu.Application.DTOs.CoachDashboard;

namespace Intervu.Application.Interfaces.UseCases.CoachDashboard
{
    public interface IGetCoachFeedbackWall
    {
        Task<List<CoachFeedbackItemDto>> ExecuteAsync(Guid coachId);
    }
}
