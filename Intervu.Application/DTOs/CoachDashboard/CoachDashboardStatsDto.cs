namespace Intervu.Application.DTOs.CoachDashboard
{
    public class CoachDashboardStatsDto
    {
        public int TotalEarnings { get; set; }
        public double EarningsGrowthPercent { get; set; }
        public int InterviewsCompleted { get; set; }
        public double InterviewsGrowthPercent { get; set; }
        public double AverageRating { get; set; }
        public double AcceptanceRate { get; set; }
        public double AcceptanceRateGrowthPercent { get; set; }
        public List<EarningsTrendPointDto> EarningsTrend { get; set; } = [];
    }
}
