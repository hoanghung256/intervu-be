using Intervu.Domain.Abstractions.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Intervu.Domain.Entities
{
    public class UserSkillAssessmentSnapshot : EntityBase<Guid>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public Guid UserId { get; set; }

        public string TargetJson { get; set; } = "{}";
        public string CurrentJson { get; set; } = "{}";
        public string GapJson { get; set; } = "{}";
        public string RoadMapJson { get; set; } = "{}";
        public string? AnswerJson { get; set; } = "{}";

        [NotMapped]
        public Target? Target
        {
            get => Deserialize<Target>(TargetJson);
            set => TargetJson = Serialize(value);
        }

        [NotMapped]
        public Current? Current
        {
            get => Deserialize<Current>(CurrentJson);
            set => CurrentJson = Serialize(value);
        }

        [NotMapped]
        public Gap? Gap
        {
            get => Deserialize<Gap>(GapJson);
            set => GapJson = Serialize(value);
        }

        [NotMapped]
        public RoadmapSnapshot? Roadmap
        {
            get => Deserialize<RoadmapSnapshot>(RoadMapJson);
            set => RoadMapJson = Serialize(value);
        }

        [NotMapped]
        public AnswerSnapshot? Answer
        {
            get => Deserialize<AnswerSnapshot>(AnswerJson);
            set => AnswerJson = Serialize(value);
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public void EnsureJsonPayloads()
        {
            TargetJson = Normalize(TargetJson);
            CurrentJson = Normalize(CurrentJson);
            GapJson = Normalize(GapJson);
            RoadMapJson = Normalize(RoadMapJson);
            AnswerJson = Normalize(AnswerJson);
        }

        private static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        private static string Normalize(string? json)
        {
            return string.IsNullOrWhiteSpace(json) ? "{}" : json;
        }
    }

    public class Target
    {
        public List<string> Roles { get; set; } = new();
        public string Level { get; set; } = string.Empty;
        public List<string> SkillsTarget { get; set; } = new();
    }

    public class SkillLevel
    {
        public string Skill { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int SfiaLevel { get; set; }
    }

    public class Current
    {
        public List<SkillLevel> Skills { get; set; } = new();
    }

    public class Gap
    {
        public List<string> Missing { get; set; } = new();
        public List<string> Weak { get; set; } = new();
    }

    public class Roadmap
    {
        public List<RoadmapPhase> Phases { get; set; } = new();
    }

    public class RoadmapPhase
    {
        public string Label { get; set; } = string.Empty;
        public int PhaseNumber { get; set; }
        public List<string> Skills { get; set; } = new();
    }

    public class RoadmapSnapshot
    {
        [JsonPropertyName("roadmap_metadata")]
        public RoadmapMetadataSnapshot RoadmapMetadata { get; set; } = new();

        [JsonPropertyName("phases")]
        public List<RoadmapPhaseSnapshot> Phases { get; set; } = new();
    }

    public class RoadmapMetadataSnapshot
    {
        [JsonPropertyName("target_role")]
        public string TargetRole { get; set; } = string.Empty;

        [JsonPropertyName("target_level")]
        public string TargetLevel { get; set; } = string.Empty;

        [JsonPropertyName("total_phases")]
        public int TotalPhases { get; set; }
    }

    public class RoadmapPhaseSnapshot
    {
        [JsonPropertyName("phase_id")]
        public string PhaseId { get; set; } = string.Empty;

        [JsonPropertyName("phase_name")]
        public string PhaseName { get; set; } = string.Empty;

        [JsonPropertyName("recommended_coaches")]
        public List<RoadmapCoachSnapshot> RecommendedCoaches { get; set; } = new();

        [JsonPropertyName("mock_history")]
        public List<RoadmapMockHistorySnapshot> MockHistory { get; set; } = new();

        [JsonPropertyName("nodes")]
        public List<RoadmapNodeSnapshot> Nodes { get; set; } = new();
    }

    public class RoadmapCoachSnapshot
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; } = string.Empty;
    }

    public class RoadmapMockHistorySnapshot
    {
        [JsonPropertyName("mock_id")]
        public string MockId { get; set; } = string.Empty;

        [JsonPropertyName("mock_title")]
        public string MockTitle { get; set; } = string.Empty;

        [JsonPropertyName("interview_type")]
        public string InterviewType { get; set; } = string.Empty;

        [JsonPropertyName("coach_name")]
        public string CoachName { get; set; } = string.Empty;

        [JsonPropertyName("interviewed_at")]
        public string InterviewedAt { get; set; } = string.Empty;

        [JsonPropertyName("evaluation")]
        public List<RoadmapEvaluationSnapshot> Evaluation { get; set; } = new();
    }

    public class RoadmapEvaluationSnapshot
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;
    }

    public class RoadmapNodeSnapshot
    {
        [JsonPropertyName("skill_id")]
        public string SkillId { get; set; } = string.Empty;

        [JsonPropertyName("skill_name")]
        public string SkillName { get; set; } = string.Empty;

        [JsonPropertyName("assessment")]
        public RoadmapNodeAssessmentSnapshot Assessment { get; set; } = new();

        [JsonPropertyName("child_skills")]
        public List<RoadmapChildSkillSnapshot> ChildSkills { get; set; } = new();
    }

    public class RoadmapNodeAssessmentSnapshot
    {
        [JsonPropertyName("current_level")]
        public string CurrentLevel { get; set; } = string.Empty;

        [JsonPropertyName("target_level")]
        public string TargetLevel { get; set; } = string.Empty;

        [JsonPropertyName("sfia_level")]
        public int SfiaLevel { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("progress")]
        public int Progress { get; set; }
    }

    public class RoadmapChildSkillSnapshot
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("questions")]
        public List<RoadmapQuestionSnapshot> Questions { get; set; } = new();
    }

    public class RoadmapQuestionSnapshot
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = string.Empty;
    }

    public class AnswerSnapshot
    {
        [JsonPropertyName("profile")]
        public AnswerProfileSnapshot Profile { get; set; } = new();

        [JsonPropertyName("responses")]
        public List<AnswerResponseSnapshot> Responses { get; set; } = new();
    }

    public class AnswerProfileSnapshot
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonPropertyName("techstack")]
        public List<string> Techstack { get; set; } = new();

        [JsonPropertyName("domain")]
        public List<string> Domain { get; set; } = new();

        [JsonPropertyName("freeText")]
        public string FreeText { get; set; } = string.Empty;
    }

    public class AnswerResponseSnapshot
    {
        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("phase")]
        public string Phase { get; set; } = string.Empty;

        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("selectedLevel")]
        public string SelectedLevel { get; set; } = string.Empty;
    }

}
