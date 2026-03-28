using System;
using System.Collections.Generic;

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
}
