using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Assessment
{
    public class SurveyResponsesDto
    {
        public Guid UserId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public List<ResponseItem> Responses { get; set; } = new();
        public SurveyTargetDto? Target { get; set; }
        public SurveyCurrentDto? Current { get; set; }
        public SurveyGapDto? Gap { get; set; }
        public SurveyRoadmapDto? Roadmap { get; set; }
    }

    public class ResponseItem
    {
        public string Phase { get; set; } = string.Empty;
        public string Skill { get; set; } = string.Empty;
        public string SelectedLevel { get; set; } = string.Empty;
    }

    public class SurveyTargetDto
    {
        public List<string> Roles { get; set; } = new();
        public string Level { get; set; } = string.Empty;
        public List<string> SkillsTarget { get; set; } = new();
    }

    public class SurveySkillLevelDto
    {
        public string Skill { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int? SfiaLevel { get; set; }
    }

    public class SurveyCurrentDto
    {
        public List<SurveySkillLevelDto> Skills { get; set; } = new();
    }

    public class SurveyGapDto
    {
        public List<string> Missing { get; set; } = new();
        public List<string> Weak { get; set; } = new();
    }

    public class SurveyRoadmapDto
    {
        [JsonPropertyName("roadmap_metadata")]
        public SurveyRoadmapMetadataDto RoadmapMetadata { get; set; } = new();

        [JsonPropertyName("phases")]
        public List<SurveyRoadmapPhaseDto> Phases { get; set; } = new();
    }

    public class SurveyRoadmapMetadataDto
    {
        [JsonPropertyName("target_role")]
        public string TargetRole { get; set; } = string.Empty;

        [JsonPropertyName("target_level")]
        public string TargetLevel { get; set; } = string.Empty;

        [JsonPropertyName("total_phases")]
        public int TotalPhases { get; set; }
    }

    public class SurveyRoadmapPhaseDto
    {
        [JsonPropertyName("phase_id")]
        public string PhaseId { get; set; } = string.Empty;

        [JsonPropertyName("phase_name")]
        public string PhaseName { get; set; } = string.Empty;

        [JsonPropertyName("recommended_coaches")]
        public List<SurveyRoadmapCoachDto> RecommendedCoaches { get; set; } = new();

        [JsonPropertyName("mock_history")]
        public List<SurveyRoadmapMockHistoryDto> MockHistory { get; set; } = new();

        [JsonPropertyName("nodes")]
        public List<SurveyRoadmapNodeDto> Nodes { get; set; } = new();
    }

    public class SurveyRoadmapCoachDto
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

    public class SurveyRoadmapMockHistoryDto
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
        public List<SurveyRoadmapEvaluationDto> Evaluation { get; set; } = new();
    }

    public class SurveyRoadmapEvaluationDto
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

    public class SurveyRoadmapNodeDto
    {
        [JsonPropertyName("skill_id")]
        public string SkillId { get; set; } = string.Empty;

        [JsonPropertyName("skill_name")]
        public string SkillName { get; set; } = string.Empty;

        [JsonPropertyName("assessment")]
        public SurveyRoadmapNodeAssessmentDto Assessment { get; set; } = new();

        [JsonPropertyName("child_skills")]
        public List<SurveyRoadmapChildSkillDto> ChildSkills { get; set; } = new();
    }

    public class SurveyRoadmapNodeAssessmentDto
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

    [JsonConverter(typeof(SurveyRoadmapChildSkillDtoJsonConverter))]
    public class SurveyRoadmapChildSkillDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("questions")]
        public List<SurveyRoadmapQuestionDto> Questions { get; set; } = new();
    }

    public class SurveyRoadmapQuestionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = string.Empty;
    }

    public class SurveyRoadmapChildSkillDtoJsonConverter : JsonConverter<SurveyRoadmapChildSkillDto>
    {
        public override SurveyRoadmapChildSkillDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new SurveyRoadmapChildSkillDto
                {
                    Name = reader.GetString() ?? string.Empty,
                    Questions = new List<SurveyRoadmapQuestionDto>()
                };
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("child_skills item must be a string or object.");
            }

            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var result = new SurveyRoadmapChildSkillDto();

            if (TryGetPropertyIgnoreCase(root, "name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                result.Name = nameElement.GetString() ?? string.Empty;
            }

            if (TryGetPropertyIgnoreCase(root, "questions", out var questionsElement) &&
                questionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var questionElement in questionsElement.EnumerateArray())
                {
                    if (questionElement.ValueKind == JsonValueKind.String)
                    {
                        result.Questions.Add(new SurveyRoadmapQuestionDto
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            Title = questionElement.GetString() ?? string.Empty,
                            Difficulty = string.Empty
                        });
                        continue;
                    }

                    if (questionElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var question = new SurveyRoadmapQuestionDto();

                    if (TryGetPropertyIgnoreCase(questionElement, "id", out var idElement))
                    {
                        question.Id = idElement.ToString();
                    }

                    if (TryGetPropertyIgnoreCase(questionElement, "title", out var titleElement))
                    {
                        question.Title = titleElement.ToString();
                    }

                    if (TryGetPropertyIgnoreCase(questionElement, "difficulty", out var difficultyElement))
                    {
                        question.Difficulty = difficultyElement.ToString();
                    }

                    result.Questions.Add(question);
                }
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, SurveyRoadmapChildSkillDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.Name ?? string.Empty);
            writer.WritePropertyName("questions");
            JsonSerializer.Serialize(writer, value.Questions ?? new List<SurveyRoadmapQuestionDto>(), options);
            writer.WriteEndObject();
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
