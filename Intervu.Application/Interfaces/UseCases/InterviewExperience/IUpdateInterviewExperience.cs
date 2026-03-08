using Intervu.Application.DTOs.InterviewExperience;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewExperience
{
    public interface IUpdateInterviewExperience
    {
        Task ExecuteAsync(Guid id, UpdateInterviewExperienceRequest request, Guid userId);
    }
}
