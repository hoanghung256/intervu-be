using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ICandidateProfileRepository : IRepositoryBase<CandidateProfile>
    {
        Task<CandidateProfile?> GetProfileBySlugAsync(string slug);
        Task<CandidateProfile?> GetProfileByIdAsync(Guid id);
        Task CreateCandidateProfileAsync(CandidateProfile profile);
        Task UpdateCandidateProfileAsync(CandidateProfile updatedProfile);
        void DeleteCandidateProfile(Guid id);
    }
}
