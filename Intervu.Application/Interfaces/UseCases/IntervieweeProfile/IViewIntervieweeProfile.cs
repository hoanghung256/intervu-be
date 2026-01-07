using Intervu.Application.DTOs.Interviewee;

namespace Intervu.Application.Interfaces.UseCases.IntervieweeProfile
{
    public interface IViewIntervieweeProfile
    {
        Task<IntervieweeProfileDto?> ViewOwnProfileAsync(Guid id);
        Task<IntervieweeViewDto?> ViewProfileBySlugAsync(string slug);
    }
}
