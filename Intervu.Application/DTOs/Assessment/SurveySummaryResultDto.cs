using System;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Assessment
{
    public class SurveySummaryResultDto
    {
        public Guid UserId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public object SummaryObject { get; set; } = new { };
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
