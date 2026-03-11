using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.InterviewExperience
{
    public interface IDeleteInterviewExperience
    {
        Task ExecuteAsync(Guid id, Guid userId);
    }
}
