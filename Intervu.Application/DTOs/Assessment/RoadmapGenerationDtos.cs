using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Intervu.Application.DTOs.Ai;

namespace Intervu.Application.DTOs.Assessment
{
    public class GenerateRoadmapFromSurveyRequestDto
    {
        public Guid UserId { get; set; }
        public bool ForceRegenerate { get; set; }
    }

    public class GenerateRoadmapResultDto
    {
        public string Status { get; set; } = "success";
        public SurveyRoadmapDto? Roadmap { get; set; }
        public string? Error { get; set; }
    }

    public class AiGenerateRoadmapRequestDto
    {
        [JsonPropertyName("target_skill")]
        public AiTargetSkillDto TargetSkill { get; set; } = new();

        [JsonPropertyName("current_level")]
        public AiCurrentLevelDto CurrentLevel { get; set; } = new();

        [JsonPropertyName("gap")]
        public AiGapDto Gap { get; set; } = new();
    }

    public class AiTargetSkillDto
    {
        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new();

        [JsonPropertyName("skillsTarget")]
        public List<string> SkillsTarget { get; set; } = new();
    }

    public class AiCurrentLevelDto
    {
        [JsonPropertyName("skills")]
        public List<AiSkillLevelDto> Skills { get; set; } = new();
    }

    public class AiSkillLevelDto
    {
        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonPropertyName("sfiaLevel")]
        public int SfiaLevel { get; set; }
    }

    public class AiGapDto
    {
        [JsonPropertyName("weak")]
        public List<string> Weak { get; set; } = new();

        [JsonPropertyName("missing")]
        public List<string> Missing { get; set; } = new();
    }

    public class AiGenerateRoadmapResponseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("roadmap")]
        public SurveyRoadmapDto? Roadmap { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("usage")]
        public LlmTokenUsageDto? Usage { get; set; }
    }

    // ── Roadmap progress update (post-interview) ────────────────────────────

    public class AiUpdateRoadmapProgressRequestDto
    {
        [JsonPropertyName("current_roadmap")]
        public SurveyRoadmapDto CurrentRoadmap { get; set; } = new();

        [JsonPropertyName("interview_type")]
        public string InterviewType { get; set; } = string.Empty;

        [JsonPropertyName("aim_level")]
        public string AimLevel { get; set; } = string.Empty;

        [JsonPropertyName("evaluation")]
        public List<AiEvaluationItemDto> Evaluation { get; set; } = new();
    }

    public class AiEvaluationItemDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;
    }

    public class AiUpdateRoadmapProgressResponseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("roadmap")]
        public SurveyRoadmapDto? Roadmap { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
