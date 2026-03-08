using Intervu.Application.DTOs.InterviewExperience;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewExperience
{
    public interface ICreateInterviewExperience
    {
        Task<Guid> ExecuteAsync(CreateInterviewExperienceRequest request, Guid userId);
    }
}
