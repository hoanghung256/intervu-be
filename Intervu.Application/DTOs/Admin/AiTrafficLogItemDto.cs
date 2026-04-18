using System;

namespace Intervu.Application.DTOs.Admin
{
    public class AiTrafficLogItemDto
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EndpointName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public long LatencyMs { get; set; }
    }
}
