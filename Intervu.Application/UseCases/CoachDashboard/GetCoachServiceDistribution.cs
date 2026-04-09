using Intervu.Application.DTOs.CoachDashboard;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachDashboard
{
    public class GetCoachServiceDistribution : IGetCoachServiceDistribution
    {
        private readonly IInterviewRoomRepository _roomRepo;

        public GetCoachServiceDistribution(IInterviewRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<List<CoachServiceDistributionDto>> ExecuteAsync(Guid coachId)
        {
            var distribution = await _roomRepo.GetServiceDistributionByCoachIdAsync(coachId);
            var total = distribution.Sum(d => d.Count);

            return distribution.Select(d => new CoachServiceDistributionDto
            {
                ServiceName = d.ServiceName,
                Count = d.Count,
                Percentage = total > 0 ? Math.Round((double)d.Count / total * 100, 1) : 0
            }).ToList();
        }
    }
}
