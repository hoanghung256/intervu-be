using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;

namespace Intervu.Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<SurveySummaryResultDto> ProcessSurveyResponsesAsync(SurveyResponsesDto request);
    }
}
