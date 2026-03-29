using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs
{
    public class GenerateAssessmentOptionsResponse
    {
        [JsonProperty("techstack")]
        [JsonPropertyName("techstack")]
        public List<string> TechStack { get; set; } = new List<string>();

        [JsonProperty("domain")]
        [JsonPropertyName("domain")]
        public List<string> Domain { get; set; } = new List<string>();
    }
}
