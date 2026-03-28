using System.Threading.Tasks;
using Intervu.Application.DTOs;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IAiService
    {
        Task<GenerateAssessmentResponse> GenerateAssessmentAsync(GenerateAssessmentRequest request);
    }
}
