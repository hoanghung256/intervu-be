using Intervu.Application.DTOs.CoachDashboard;

namespace Intervu.Application.Interfaces.UseCases.CoachDashboard
{
    public interface IGetCoachServiceDistribution
    {
        Task<List<CoachServiceDistributionDto>> ExecuteAsync(Guid coachId);
    }
}
