using Intervu.Application.DTOs.Candidate;
using Intervu.Domain.Entities.Constants;
// ﻿using Intervu.Application.DTOs.Interviewer;


namespace Intervu.Application.Interfaces.UseCases.CandidateProfile
{
    public interface IUpdateCandidateProfile
    {
        Task<CandidateProfileDto> UpdateCandidateProfileAsync(Guid id, CandidateUpdateDto updateDto);
        Task<CandidateViewDto> UpdateCandidateStatusAsync(Guid id, UserStatus status);
        Task<Domain.Entities.CandidateProfile> UpdateCandidateProfile(Guid id, string cvUrl);

    }
}
