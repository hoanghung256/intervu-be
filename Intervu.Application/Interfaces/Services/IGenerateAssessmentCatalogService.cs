using System.Threading.Tasks;
using Intervu.Application.DTOs;

namespace Intervu.Application.Interfaces.Services
{
    public interface IGenerateAssessmentCatalogService
    {
        Task<GenerateAssessmentOptionsResponse> GetOptionsAsync();
    }
}
