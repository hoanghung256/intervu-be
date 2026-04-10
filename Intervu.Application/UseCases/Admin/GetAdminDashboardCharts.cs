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
            var candidatesByDay = await _userRepository.GetRegistrationTrendAsync(start, end, UserRole.Candidate);
            var coachesByDay = await _userRepository.GetRegistrationTrendAsync(start, end, UserRole.Coach);

            // Merge by date
            var allDates = candidatesByDay.Select(x => x.Date)
                .Union(coachesByDay.Select(x => x.Date))
                .OrderBy(d => d)
                .ToList();

            var userGrowth = allDates.Select(d => new UserGrowthTrendDto
            {
                Date = d.ToString("MMM"), // E.g., "Jan", actually should be daily but screenshot shows monthly
                Candidates = candidatesByDay.Where(c => c.Date == d).Select(c => c.Count).FirstOrDefault(),
                Coaches = coachesByDay.Where(c => c.Date == d).Select(c => c.Count).FirstOrDefault()
            }).ToList();

            return (revenue, userGrowth);
        }
    }
}
