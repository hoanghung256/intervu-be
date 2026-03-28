using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Assessment
{
    public class AssessmentProcessRequest
    {
        [JsonProperty("userId")]
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("assessmentName")]
        [JsonPropertyName("assessmentName")]
        public string AssessmentName { get; set; } = string.Empty;

        [JsonProperty("Target")]
        [JsonPropertyName("Target")]
        public TargetDto Target { get; set; } = new TargetDto();

        [JsonProperty("Current")]
        [JsonPropertyName("Current")]
        public CurrentDto Current { get; set; } = new CurrentDto();

        [JsonProperty("Gap")]
        [JsonPropertyName("Gap")]
        public GapDto Gap { get; set; } = new GapDto();
    }

    public class TargetDto
    {
        [JsonProperty("level")]
        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonProperty("roles")]
        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [JsonProperty("skillsTarget")]
        [JsonPropertyName("skillsTarget")]
        public List<string> SkillsTarget { get; set; } = new List<string>();
    }

    public class CurrentDto
    {
        [JsonProperty("skills")]
        [JsonPropertyName("skills")]
        public List<SkillLevelDto> Skills { get; set; } = new List<SkillLevelDto>();
    }

    public class SkillLevelDto
    {
        [JsonProperty("skill")]
        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonProperty("level")]
        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;
    }

    public class GapDto
    {
        [JsonProperty("skills")]
        [JsonPropertyName("skills")]
        public List<GapSkillDto> Skills { get; set; } = new List<GapSkillDto>();
    }

    public class GapSkillDto
    {
        [JsonProperty("skill")]
        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonProperty("current")]
        [JsonPropertyName("current")]
        public string Current { get; set; } = string.Empty;

        [JsonProperty("target")]
        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty;
    }
}
