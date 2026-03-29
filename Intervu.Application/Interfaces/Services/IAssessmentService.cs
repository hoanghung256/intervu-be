using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;

namespace Intervu.Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<SurveySummaryResultDto> ProcessSurveyResponsesAsync(SurveyResponsesDto request);
        Task<UserSkillAssessmentSnapshotDto?> GetUserSkillAssessmentSnapshotAsync(Guid userId);
        Task<GenerateRoadmapResultDto> GenerateRoadmapFromSurveyAsync(Guid userId, bool forceRegenerate = false);
        Task<SurveyRoadmapDto?> GetRoadmapByUserIdAsync(Guid userId);
    }
}
