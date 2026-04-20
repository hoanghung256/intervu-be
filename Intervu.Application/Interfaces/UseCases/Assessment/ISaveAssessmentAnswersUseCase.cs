using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;
using System;

namespace Intervu.Application.Interfaces.UseCases.Assessment
{
    public interface ISaveAssessmentAnswersUseCase
    {
        Task<SaveAssessmentAnswersResultDto> ExecuteAsync(SaveAssessmentAnswersRequestDto request);
        Task<SaveAssessmentAnswersResultDto> SaveRawEvaluationAsync(Guid userId, string rawEvaluationJson);
    }
}
