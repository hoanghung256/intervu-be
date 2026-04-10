using System;

namespace Intervu.Application.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public double TotalUsersGrowth { get; set; }
        public int ActiveUsers30D { get; set; }
        public double ActiveUsersGrowth { get; set; }
        public decimal TotalRevenue { get; set; }
        public double RevenueGrowth { get; set; }
        public double RefundRate { get; set; }
        public double RefundRateGrowth { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalCoaches { get; set; }
        public int TotalPayments { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
    }
}
