namespace Intervu.Application.DTOs.CoachDashboard
{
    public class CoachAvailabilityOverviewDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public List<string> TimeSlots { get; set; } = [];
    }
}
