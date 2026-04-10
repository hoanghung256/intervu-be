using System.Collections.Generic;
using System.Text.Json.Serialization;
using Intervu.Application.Utils;
using Newtonsoft.Json;

namespace Intervu.Application.DTOs
{
    public class GenerateAssessmentRequest
    {
        [JsonProperty("role")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("level")]
        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonProperty("techstack")]
        [JsonPropertyName("techstack")]
        [Newtonsoft.Json.JsonConverter(typeof(SingleOrArrayNewtonsoftConverter<string>))]
        [System.Text.Json.Serialization.JsonConverter(typeof(SingleOrArraySystemTextJsonConverter<string>))]
        public List<string> TechStack { get; set; } = new List<string>();

        [JsonProperty("domain")]
        [JsonPropertyName("domain")]
        [Newtonsoft.Json.JsonConverter(typeof(SingleOrArrayNewtonsoftConverter<string>))]
        [System.Text.Json.Serialization.JsonConverter(typeof(SingleOrArraySystemTextJsonConverter<string>))]
        public List<string> Domain { get; set; } = new List<string>();

        [JsonProperty("freeText")]
        [JsonPropertyName("free_text")]
        public string FreeText { get; set; } = string.Empty;
    }
}
