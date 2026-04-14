using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Assessment
{
    public class SaveAssessmentAnswersRequestDto
    {
        public Guid UserId { get; set; }
        public AssessmentProfileDto Profile { get; set; } = new();
        public List<AssessmentResponseDetailDto> Responses { get; set; } = new();
        public List<DerivedSkillDto> DerivedSkills { get; set; } = new();
        public AssessmentProcessingPayloadDto? ProcessingPayload { get; set; }
    }

    public class SaveAssessmentAnswersResultDto
    {
        public Guid UserId { get; set; }
        public DateTime SavedAtUtc { get; set; }
    }

    public class AssessmentProfileDto
    {
        public string Role { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public List<string> Techstack { get; set; } = new();
        public List<string> Domain { get; set; } = new();
        public string FreeText { get; set; } = string.Empty;
    }

    public class AssessmentResponseDetailDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string Skill { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string SelectedLevel { get; set; } = string.Empty;
    }

    public class DerivedSkillDto
    {
        public string SkillKey { get; set; } = string.Empty;
        public string SelectedLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class AssessmentProcessingPayloadDto
    {
        public Guid UserId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public List<ResponseItem>? Responses { get; set; } = new();
        public SurveyTargetDto? Target { get; set; }
        public SurveyCurrentDto? Current { get; set; }
        public SurveyGapDto? Gap { get; set; }
    }
}
