using System.Collections.Generic;

namespace Intervu.Application.DTOs.Admin
{
    public class PythonAiMetricsDto
    {
        public int TotalRequests { get; set; }
        public long TotalPromptTokens { get; set; }
        public long TotalCompletionTokens { get; set; }
        public long TotalTokens { get; set; }
        public double AverageLatencyMs { get; set; }
        public int ServiceCount { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<string> AvailableProviders { get; set; } = new();
        public List<string> AvailableEndpoints { get; set; } = new();
        public List<AiTrafficLogItemDto> Logs { get; set; } = new();
        public List<AiEndpointSeriesPointDto> EndpointSeries { get; set; } = new();
        public List<string> SeriesEndpoints { get; set; } = new();
        public string SeriesBucket { get; set; } = "hour";
    }
}
