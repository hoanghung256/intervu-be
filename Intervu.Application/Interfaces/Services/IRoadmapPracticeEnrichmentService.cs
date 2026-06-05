using System.Threading;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Assessment;

namespace Intervu.Application.Interfaces.Services
{
    /// <summary>
    /// Links roadmap child_skill rows to approved question-bank items via keyword search.
    /// </summary>
    public interface IRoadmapPracticeEnrichmentService
    {
        Task EnrichChildSkillQuestionsAsync(SurveyRoadmapDto? roadmap, CancellationToken cancellationToken = default);
    }
}
