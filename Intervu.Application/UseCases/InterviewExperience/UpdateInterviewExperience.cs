using AutoMapper;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewExperience
{
public class UpdateInterviewExperience(IUnitOfWork unitOfWork, IMapper mapper)
        : IUpdateInterviewExperience
    {
        public async Task ExecuteAsync(Guid id, UpdateInterviewExperienceRequest request, Guid userId)
        {
            var repo = unitOfWork.GetRepository<IInterviewExperienceRepository>();
            var experience = await repo.GetByIdAsync(id)
                ?? throw new Exception("Interview experience not found");

            experience.CompanyName = request.CompanyName;
            experience.Role = request.Role;
            experience.Level = request.Level;
            experience.LastRoundCompleted = request.LastRoundCompleted;
            experience.InterviewProcess = request.InterviewProcess;
            experience.IsInterestedInContact = request.IsInterestedInContact;
            experience.UpdatedBy = userId;
            experience.UpdatedAt = DateTime.UtcNow;

            repo.UpdateAsync(experience);
            await unitOfWork.SaveChangesAsync();
        }
    }
}