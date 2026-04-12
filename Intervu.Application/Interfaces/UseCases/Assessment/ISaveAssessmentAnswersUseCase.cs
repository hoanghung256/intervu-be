using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;

namespace Intervu.Application.Interfaces.UseCases.Assessment
{
    public interface ISaveAssessmentAnswersUseCase
    {
        Task<SaveAssessmentAnswersResultDto> ExecuteAsync(SaveAssessmentAnswersRequestDto request);
    }
}
