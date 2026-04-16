using System;

namespace Intervu.Application.DTOs.Admin
{
    public class ServiceHealthDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;   // "Healthy" | "Degraded" | "Unreachable" | "Configured" | "KeyMissing"
        public string Endpoint { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public long TotalRequestsSinceStartup { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}
