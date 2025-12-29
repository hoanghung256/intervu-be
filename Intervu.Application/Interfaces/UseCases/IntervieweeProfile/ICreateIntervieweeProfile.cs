using Intervu.Application.DTOs.Interviewee;

namespace Intervu.Application.Interfaces.UseCases.IntervieweeProfile
{
    public interface ICreateIntervieweeProfile
    {
        Task<IntervieweeProfileDto> CreateIntervieweeProfileAsync(IntervieweeCreateDto createDto);
    }
}
