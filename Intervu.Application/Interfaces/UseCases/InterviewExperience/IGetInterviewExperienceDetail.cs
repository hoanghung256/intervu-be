using Intervu.Application.DTOs.InterviewExperience;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewExperience
{
    public interface IGetInterviewExperienceDetail
    {
        Task<InterviewExperienceDetailDto?> ExecuteAsync(Guid id);
    }
}
