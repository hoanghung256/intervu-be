using Intervu.Application.DTOs.CoachDashboard;

namespace Intervu.Application.Interfaces.UseCases.CoachDashboard
{
    public interface IGetCoachPendingRequests
    {
        Task<List<CoachPendingRequestDto>> ExecuteAsync(Guid coachId);
    }
}
