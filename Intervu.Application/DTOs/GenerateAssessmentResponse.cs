using Intervu.Application.DTOs.Assessment;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs
{
    public class GenerateAssessmentResponse
    {
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("question")]
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonProperty("context_question")]
        [JsonPropertyName("context_question")]
        public string ContextQuestion { get; set; } = string.Empty;

        // Matches example: "phaseA"
        [JsonProperty("phaseA")]
        [JsonPropertyName("phaseA")]
        public List<AssessmentQuestionItemDto> PhaseA { get; set; } = new List<AssessmentQuestionItemDto>();

        // Matches example: "phaseB"
        [JsonProperty("phaseB")]
        [JsonPropertyName("phaseB")]
        public List<AssessmentQuestionItemDto> PhaseB { get; set; } = new List<AssessmentQuestionItemDto>();
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
