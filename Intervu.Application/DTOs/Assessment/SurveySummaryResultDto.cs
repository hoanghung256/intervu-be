using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Assessment
{
    public class EvaluateAssessmentRequestDto
    {
        [JsonPropertyName("answer")]
        public SurveyAnswerJsonDto Answer { get; set; } = new();
    }

    public class SurveySummaryResultDto
    {
        public Guid? UserId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public object Answer { get; set; } = new();
        public object Target { get; set; } = new();
        public SurveyCurrentResultDto Current { get; set; } = new();
        public List<string> Missing { get; set; } = new();
    }

    public class SurveyCurrentResultDto
    {
        public List<SurveyCurrentSkillDto> Skills { get; set; } = new();
    }

    public class SurveyCurrentSkillDto
    {
        public string Skill { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class UserSkillAssessmentSnapshotDto
    {
        public Guid UserId { get; set; }
        public string? Target { get; set; }
        public string? Current { get; set; }
        public string? Gap { get; set; }
        public string? AnswerJson { get; set; }
    }
}
