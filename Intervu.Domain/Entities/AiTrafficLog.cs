using System;
using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class AiTrafficLog : EntityBase<Guid>
    {
        public DateTime Timestamp { get; set; }
        public string EndpointName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public long LatencyMs { get; set; }
    }
}
