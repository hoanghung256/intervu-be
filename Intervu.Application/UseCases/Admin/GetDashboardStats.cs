using Intervu.Application.DTOs.Admin;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetDashboardStats : IGetDashboardStats
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFeedbackRepository _feedbackRepository;

        public GetDashboardStats(
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ICoachProfileRepository coachProfileRepository,
            ITransactionRepository transactionRepository,
            IFeedbackRepository feedbackRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _coachProfileRepository = coachProfileRepository;
            _transactionRepository = transactionRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<DashboardStatsDto> ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            var sixtyDaysAgo = now.AddDays(-60);

            // 1. Total Users & Growth
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var usersLast30D = await _userRepository.GetRegistrationsCountAsync(thirtyDaysAgo, now);
            var usersPrev30D = await _userRepository.GetRegistrationsCountAsync(sixtyDaysAgo, thirtyDaysAgo);
            var usersGrowth = usersPrev30D == 0 ? (usersLast30D > 0 ? 100 : 0) : (double)(usersLast30D - usersPrev30D) / usersPrev30D * 100;

            // 2. Active Users (MAU) & Growth
            var activeUsers = await _userRepository.GetActiveUsersCountAsync(thirtyDaysAgo);
            var activeUsersPrev = await _userRepository.GetActiveUsersCountAsync(sixtyDaysAgo);
            // growth of MAU compared to previous window MAU
            var activeGrowth = activeUsersPrev == 0 ? (activeUsers > 0 ? 100 : 0) : (double)(activeUsers - activeUsersPrev) / activeUsersPrev * 100;

            // 3. Revenue & Growth
            var revenueLast30D = await _transactionRepository.GetTotalRevenueAsync(thirtyDaysAgo, now);
            var revenuePrev30D = await _transactionRepository.GetTotalRevenueAsync(sixtyDaysAgo, thirtyDaysAgo);
            var revenueGrowth = revenuePrev30D == 0 ? (revenueLast30D > 0 ? 100 : 0) : (double)(revenueLast30D - revenuePrev30D) / (double)revenuePrev30D * 100;

            // 4. Refund Rate
            var refundsCount = await _transactionRepository.GetRefundCountAsync(thirtyDaysAgo, now);
            // For a real rate we'd need total transaction count, but we'll use a simplified metric
            double refundRate = totalUsers > 0 ? (double)refundsCount / totalUsers * 100 : 0;

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalUsersGrowth = Math.Round(usersGrowth, 1),
                ActiveUsers30D = activeUsers,
                ActiveUsersGrowth = Math.Round(activeGrowth, 1),
                TotalRevenue = revenueLast30D,
                RevenueGrowth = Math.Round(revenueGrowth, 1),
                RefundRate = Math.Round(refundRate, 2),
                RefundRateGrowth = -0.5, // Trend placeholder
                TotalCompanies = await _companyRepository.GetTotalCompaniesCountAsync(),
                TotalCoaches = await _coachProfileRepository.GetTotalCoachCountAsync(),
                TotalFeedbacks = await _feedbackRepository.GetTotalFeedbacksCountAsync(),
                AverageRating = await _feedbackRepository.GetAverageRatingAsync()
            };
        }
    }
}
