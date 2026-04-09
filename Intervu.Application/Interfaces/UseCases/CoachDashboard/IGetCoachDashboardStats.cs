using Intervu.Application.DTOs.CoachDashboard;

namespace Intervu.Application.Interfaces.UseCases.CoachDashboard
{
    public interface IGetCoachDashboardStats
    {
        Task<CoachDashboardStatsDto> ExecuteAsync(Guid coachId, string period);
    }
}
