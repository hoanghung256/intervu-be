using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Ai
{
    public class AiCvEvaluationResponseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new();

        [JsonPropertyName("gaps")]
        public List<string> Gaps { get; set; } = new();

        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; } = string.Empty;

        [JsonPropertyName("final_verdict")]
        public string FinalVerdict { get; set; } = string.Empty;

        [JsonIgnore]
        public string? Error { get; set; }
    }
}
