using Intervu.Application.DTOs.Interviewee;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.IntervieweeProfile
{
    public interface IUpdateIntervieweeProfile
    {
        Task<IntervieweeProfileDto> UpdateIntervieweeProfileAsync(Guid id, IntervieweeUpdateDto updateDto);
        Task<IntervieweeViewDto> UpdateIntervieweeStatusAsync(Guid id, UserStatus status);
    }
}
