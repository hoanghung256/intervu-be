using Intervu.Application.DTOs.Assessment;
using Intervu.Application.DTOs.Ai;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs
{
    public class GenerateAssessmentResponse
    {
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("assessment")]
        [JsonPropertyName("assessment")]
        public JToken? Assessment { get; set; }

        [JsonProperty("question")]
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonProperty("context_question")]
        [JsonPropertyName("context_question")]
        public string ContextQuestion { get; set; } = string.Empty;

        [JsonProperty("contextQuestion")]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ContextQuestionCamelCase
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ContextQuestion = value;
                }
            }
        }

        [JsonProperty("phaseA")]
        [JsonPropertyName("phaseA")]
        public JArray PhaseA { get; set; } = new JArray();

        [JsonProperty("phaseB")]
        [JsonPropertyName("phaseB")]
        public JArray PhaseB { get; set; } = new JArray();

        [JsonProperty("usage")]
        [JsonPropertyName("usage")]
        public LlmTokenUsageDto? Usage { get; set; }
    }

    public class AssessmentQuestionItemDto
    {
        [JsonProperty("questionId")]
        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonProperty("skill")]
        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonProperty("question")]
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonProperty("options")]
        [JsonPropertyName("options")]
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
    }

    public class OptionDto
    {
        [JsonProperty("text")]
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("level")]
        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;
    }
}
