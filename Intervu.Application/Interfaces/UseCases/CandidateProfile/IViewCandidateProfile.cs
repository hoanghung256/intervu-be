using Intervu.Application.DTOs.Candidate;

namespace Intervu.Application.Interfaces.UseCases.CandidateProfile
{
    public interface IViewCandidateProfile
    {
        Task<CandidateProfileDto?> ViewOwnProfileAsync(Guid id);
        Task<CandidateViewDto?> ViewProfileBySlugAsync(string slug);
    }
}
