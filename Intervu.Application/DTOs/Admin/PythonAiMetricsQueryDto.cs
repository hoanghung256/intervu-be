using System;

namespace Intervu.Application.DTOs.Admin
{
    public class PythonAiMetricsQueryDto
    {
        public string? Timeframe { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Provider { get; set; }
        public string? Endpoint { get; set; }
        public string? UseCase { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
