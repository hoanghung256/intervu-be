using Intervu.Application.DTOs.CoachDashboard;

namespace Intervu.Application.Interfaces.UseCases.CoachDashboard
{
    public interface IGetCoachUpcomingSessions
    {
        Task<List<CoachUpcomingSessionDto>> ExecuteAsync(Guid coachId);
    }
}
