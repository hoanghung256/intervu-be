using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewExperience;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewExperience
{
    public interface IGetInterviewExperiences
    {
        Task<PagedResult<InterviewExperienceSummaryDto>> ExecuteAsync(InterviewExperienceFilterRequest filter);
    }
}
