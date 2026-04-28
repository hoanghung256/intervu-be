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

        [JsonPropertyName("coach_catalog")]
        public List<AiCoachCatalogEntryDto> CoachCatalog { get; set; } = new();

        /// <summary>
        /// Pass-through of the snapshot's AnswerJson (per-question score, desc,
        /// confidence, etc.). The AI service's strategy step uses it to add
        /// per-mission interview themes and failed-question excerpts. Optional —
        /// the AI service still produces a valid roadmap when this is null.
        /// </summary>
        [JsonPropertyName("answer_json")]
        public object? AnswerJson { get; set; }
    }

    public class AiCoachCatalogEntryDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug_profile_url")]
        public string SlugProfileUrl { get; set; } = string.Empty;

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = new();

        [JsonPropertyName("bio")]
        public string Bio { get; set; } = string.Empty;

        [JsonPropertyName("services")]
        public List<AiCoachCatalogServiceDto> Services { get; set; } = new();
    }

    public class AiCoachCatalogServiceDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("interview_type_name")]
        public string InterviewTypeName { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("aim_level_hint")]
        public string AimLevelHint { get; set; } = string.Empty;
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

        [JsonPropertyName("score")]
        public decimal Score { get; set; }
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

        /// <summary>
        /// Optional: when set, AI service updates only the node with matching skill_id
        /// using the deterministic progress formula (no LLM).
        /// </summary>
        [JsonPropertyName("target_node_id")]
        public string? TargetNodeId { get; set; }
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

    // ── Role × level competency matrix (Phase 2) ────────────────────────────

    public class AiCompetencyMatrixResponseDto
    {
        [JsonPropertyName("role_key")]
        public string RoleKey { get; set; } = string.Empty;

        [JsonPropertyName("level_key")]
        public string LevelKey { get; set; } = string.Empty;

        [JsonPropertyName("skills")]
        public List<AiSkillCompetencyDto> Skills { get; set; } = new();
    }

    public class AiSkillCompetencyDto
    {
        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonPropertyName("target_level")]
        public int TargetLevel { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; } = 0.7;

        [JsonPropertyName("interview_critical")]
        public bool InterviewCritical { get; set; }
    }
}
