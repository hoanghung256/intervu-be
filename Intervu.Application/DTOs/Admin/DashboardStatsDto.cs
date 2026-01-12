using System;

namespace Intervu.Application.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalCoaches { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
    }
}
