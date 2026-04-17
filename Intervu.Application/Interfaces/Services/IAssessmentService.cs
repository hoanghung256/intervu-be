using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;

namespace Intervu.Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<SurveySummaryResultDto> ProcessSurveyResponsesAsync(SurveyResponsesDto request, CancellationToken cancellationToken = default);
        Task<UserSkillAssessmentSnapshotDto?> GetUserSkillAssessmentSnapshotAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<GenerateRoadmapResultDto> GenerateRoadmapFromSurveyAsync(Guid userId, bool forceRegenerate = false, CancellationToken cancellationToken = default);
        Task<SurveyRoadmapDto?> GetRoadmapByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpdateRoadmapAfterInterviewAsync(Guid candidateId, Guid interviewRoomId, string coachName, CancellationToken cancellationToken = default);
    }
}
