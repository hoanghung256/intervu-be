using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAdminDashboardCharts : IGetAdminDashboardCharts
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public GetAdminDashboardCharts(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<(List<RevenueTrendDto> Revenue, List<UserGrowthTrendDto> UserGrowth)> ExecuteAsync()
        {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-30);

            // 1. Revenue Chart
            var revTrend = await _transactionRepository.GetDailyRevenueTrendAsync(start, end);
            var revenue = revTrend.Select(r => new RevenueTrendDto
            {
                Date = r.Date.ToString("ddd"), // E.g., "Mon"
                Amount = r.Amount
            }).ToList();

            // 2. User Growth Chart 
            var monthRangeStart = new DateTime(end.Year, end.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
            var candidatesByDay = await _userRepository.GetRegistrationTrendAsync(monthRangeStart, end, UserRole.Candidate);
            var coachesByDay = await _userRepository.GetRegistrationTrendAsync(monthRangeStart, end, UserRole.Coach);

            var candidatesByMonth = candidatesByDay
                .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1, 0, 0, 0, DateTimeKind.Utc))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            var coachesByMonth = coachesByDay
                .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1, 0, 0, 0, DateTimeKind.Utc))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            var monthStarts = Enumerable.Range(0, 6)
                .Select(i => monthRangeStart.AddMonths(i))
                .ToList();

            var userGrowth = monthStarts.Select(monthStart => new UserGrowthTrendDto
            {
                Date = monthStart.ToString("MMM yyyy"),
                Candidates = candidatesByMonth.TryGetValue(monthStart, out var c) ? c : 0,
                Coaches = coachesByMonth.TryGetValue(monthStart, out var k) ? k : 0,
            }).ToList();

            return (revenue, userGrowth);
        }
    }
}
