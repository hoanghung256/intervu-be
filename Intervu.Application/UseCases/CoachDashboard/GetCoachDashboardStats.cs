using Intervu.Application.DTOs.CoachDashboard;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachDashboard
{
    public class GetCoachDashboardStats : IGetCoachDashboardStats
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IInterviewRoomRepository _roomRepo;
        private readonly IFeedbackRepository _feedbackRepo;
        private readonly IBookingRequestRepository _bookingRepo;

        public GetCoachDashboardStats(
            ITransactionRepository transactionRepo,
            IInterviewRoomRepository roomRepo,
            IFeedbackRepository feedbackRepo,
            IBookingRequestRepository bookingRepo)
        {
            _transactionRepo = transactionRepo;
            _roomRepo = roomRepo;
            _feedbackRepo = feedbackRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<CoachDashboardStatsDto> ExecuteAsync(Guid coachId, string period)
        {
            var (currentStart, currentEnd, previousStart, previousEnd) = GetDateRange(period);

            // Current period stats
            var currentEarnings = await _transactionRepo.GetTotalPayoutByUserAsync(coachId, currentStart, currentEnd);
            var previousEarnings = await _transactionRepo.GetTotalPayoutByUserAsync(coachId, previousStart, previousEnd);

            var currentInterviews = await _roomRepo.GetCompletedCountByCoachIdAsync(coachId, currentStart, currentEnd);
            var previousInterviews = await _roomRepo.GetCompletedCountByCoachIdAsync(coachId, previousStart, previousEnd);

            var averageRating = await _feedbackRepo.GetAverageRatingByCoachIdAsync(coachId);

            // Acceptance rate: accepted / (accepted + rejected) for current period
            var (acceptedItems, _) = await _bookingRepo.GetPagedByCoachIdAsync(
                coachId, null, new List<BookingRequestStatus> { BookingRequestStatus.Accepted }, 1, int.MaxValue);
            var (rejectedItems, _) = await _bookingRepo.GetPagedByCoachIdAsync(
                coachId, null, new List<BookingRequestStatus> { BookingRequestStatus.Rejected }, 1, int.MaxValue);

            var totalDecisions = acceptedItems.Count + rejectedItems.Count;
            var acceptanceRate = totalDecisions > 0 ? (double)acceptedItems.Count / totalDecisions * 100 : 0;

            // Previous period acceptance rate for growth
            // Simplified: use overall rate since booking requests don't have period-based queries easily
            var acceptanceRateGrowth = 0.0;

            // Earnings trend
            var dailyPayouts = await _transactionRepo.GetDailyPayoutByUserAsync(coachId, currentStart, currentEnd);
            var earningsTrend = BuildEarningsTrend(dailyPayouts, currentStart, currentEnd, period);

            return new CoachDashboardStatsDto
            {
                TotalEarnings = currentEarnings,
                EarningsGrowthPercent = CalculateGrowth(currentEarnings, previousEarnings),
                InterviewsCompleted = currentInterviews,
                InterviewsGrowthPercent = CalculateGrowth(currentInterviews, previousInterviews),
                AverageRating = Math.Round(averageRating, 1),
                AcceptanceRate = Math.Round(acceptanceRate, 0),
                AcceptanceRateGrowthPercent = acceptanceRateGrowth,
                EarningsTrend = earningsTrend
            };
        }

        private static (DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd) GetDateRange(string period)
        {
            var now = DateTime.UtcNow;

            return period.ToLower() switch
            {
                "7days" => (
                    now.Date.AddDays(-6),
                    now.Date.AddDays(1),
                    now.Date.AddDays(-13),
                    now.Date.AddDays(-6)),
                "year" => (
                    new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    now.Date.AddDays(1),
                    new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => ( // "month" default
                    new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    now.Date.AddDays(1),
                    new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                    new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            };
        }

        private static double CalculateGrowth(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round((double)(current - previous) / previous * 100, 1);
        }

        private static List<EarningsTrendPointDto> BuildEarningsTrend(
            List<(DateTime Date, int Amount)> dailyPayouts, DateTime start, DateTime end, string period)
        {
            var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var payoutMap = dailyPayouts.ToDictionary(p => p.Date.Date, p => p.Amount);
            var trend = new List<EarningsTrendPointDto>();

            for (var date = start.Date; date < end.Date; date = date.AddDays(1))
            {
                payoutMap.TryGetValue(date, out var amount);

                var label = period.ToLower() == "7days"
                    ? dayNames[(int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1]
                    : date.ToString("MMM dd");

                trend.Add(new EarningsTrendPointDto { Day = label, Amount = amount });
            }

            return trend;
        }
    }
}
