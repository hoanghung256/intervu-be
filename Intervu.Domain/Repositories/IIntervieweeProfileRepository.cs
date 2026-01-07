using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IIntervieweeProfileRepository : IRepositoryBase<IntervieweeProfile>
    {
        Task<IntervieweeProfile?> GetProfileBySlugAsync(string slug);
        Task<IntervieweeProfile?> GetProfileByIdAsync(Guid id);
        Task CreateIntervieweeProfileAsync(IntervieweeProfile profile);
        Task UpdateIntervieweeProfileAsync(IntervieweeProfile updatedProfile);
        void DeleteIntervieweeProfile(Guid id);
    }
}
