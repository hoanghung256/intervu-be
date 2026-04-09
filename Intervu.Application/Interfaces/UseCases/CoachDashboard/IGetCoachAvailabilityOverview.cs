using Intervu.Application.DTOs.CoachDashboard;

namespace Intervu.Application.Interfaces.UseCases.CoachDashboard
{
    public interface IGetCoachAvailabilityOverview
    {
        Task<List<CoachAvailabilityOverviewDto>> ExecuteAsync(Guid coachId);
    }
}
