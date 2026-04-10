using Intervu.Application.DTOs.CoachDashboard;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachDashboard
{
    public class GetCoachAvailabilityOverview : IGetCoachAvailabilityOverview
    {
        private readonly ICoachAvailabilitiesRepository _availabilityRepo;

        public GetCoachAvailabilityOverview(ICoachAvailabilitiesRepository availabilityRepo)
        {
            _availabilityRepo = availabilityRepo;
        }

        public async Task<List<CoachAvailabilityOverviewDto>> ExecuteAsync(Guid coachId)
        {
            var now = DateTime.UtcNow;
            var weekStart = now.Date;
            var weekEnd = weekStart.AddDays(7);

            var blocks = await _availabilityRepo.GetBlocksInRangeAsync(coachId, weekStart, weekEnd);

            var availableBlocks = blocks
                .Where(b => b.Status == CoachAvailabilityStatus.Available)
                .ToList();

            var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            var grouped = availableBlocks
                .GroupBy(b => b.StartTime.DayOfWeek)
                .Select(g =>
                {
                    var dayIndex = g.Key == DayOfWeek.Sunday ? 6 : (int)g.Key - 1;
                    return new CoachAvailabilityOverviewDto
                    {
                        DayOfWeek = dayNames[dayIndex],
                        TimeSlots = g
                            .Select(b => b.StartTime.ToString("HH:mm"))
                            .Distinct()
                            .OrderBy(t => t)
                            .ToList()
                    };
                })
                .ToList();

            // Ensure all weekdays are present
            var result = new List<CoachAvailabilityOverviewDto>();
            for (var date = weekStart; date < weekEnd; date = date.AddDays(1))
            {
                var dayIndex = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
                var dayName = dayNames[dayIndex];
                var existing = grouped.FirstOrDefault(g => g.DayOfWeek == dayName);
                result.Add(existing ?? new CoachAvailabilityOverviewDto
                {
                    DayOfWeek = dayName,
                    TimeSlots = []
                });
            }

            return result;
        }
    }
}
