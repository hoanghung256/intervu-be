using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class RevenueTrendDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class UserGrowthTrendDto
    {
        public string Date { get; set; } = string.Empty;
        public int Candidates { get; set; }
        public int Coaches { get; set; }
    }

    public class NeedsAttentionItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // High, Medium, Low
        public string TimeOffset { get; set; } = string.Empty; // e.g., "2h ago"
        public string ActionLink { get; set; } = string.Empty;
    }

    public class CoachPerformanceDto
    {
        public int Rank { get; set; }

        /// <summary>Coach profile id (same id used by rating and public profile APIs).</summary>
        public Guid CoachId { get; set; }

        /// <summary>Public profile slug for deep links.</summary>
        public string? SlugProfileUrl { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public double Rating { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AdminDashboardDataDto
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public List<RevenueTrendDto> RevenueTrend { get; set; } = new();
        public List<UserGrowthTrendDto> UserGrowth { get; set; } = new();
        public List<NeedsAttentionItemDto> AttentionQueue { get; set; } = new();
        public List<CoachPerformanceDto> TopCoaches { get; set; } = new();
    }
}
