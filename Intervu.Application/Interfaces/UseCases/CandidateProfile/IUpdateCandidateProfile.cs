using Intervu.Application.DTOs.Candidate;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.CandidateProfile
{
    public interface IUpdateCandidateProfile
    {
        Task<CandidateProfileDto> UpdateCandidateProfileAsync(Guid id, CandidateUpdateDto updateDto);
        Task<CandidateViewDto> UpdateCandidateStatusAsync(Guid id, UserStatus status);
        Task<Domain.Entities.CandidateProfile> UpdateCandidateCVProfile(Guid id, string cvUrl);
        Task<CandidateProfileDto> UpdateCandidateWorkExperiencesAsync(Guid id, List<CandidateWorkExperienceDto> workExperiences);
    }
}
