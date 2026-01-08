using Intervu.Application.DTOs.Candidate;

namespace Intervu.Application.Interfaces.UseCases.CandidateProfile
{
    public interface ICreateCandidateProfile
    {
        Task<CandidateProfileDto> CreateCandidateProfileAsync(CandidateCreateDto createDto);
    }
}
