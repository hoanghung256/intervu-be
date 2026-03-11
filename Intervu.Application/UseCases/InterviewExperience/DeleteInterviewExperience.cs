using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewExperience
{
    public class DeleteInterviewExperience(IUnitOfWork unitOfWork)
        : IDeleteInterviewExperience
    {
        public async Task ExecuteAsync(Guid id, Guid userId)
        {
            var repo = unitOfWork.GetRepository<IInterviewExperienceRepository>();
            var experience = await repo.GetByIdAsync(id)
                ?? throw new Exception("Interview experience not found");

            repo.DeleteAsync(experience);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
