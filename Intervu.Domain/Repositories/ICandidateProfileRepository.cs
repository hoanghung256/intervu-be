using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ICandidateProfileRepository : IRepositoryBase<CandidateProfile>
    {
        Task<CandidateProfile?> GetProfileBySlugAsync(string slug);
        Task<CandidateProfile?> GetProfileByIdAsync(Guid id);
        Task CreateCandidateProfileAsync(CandidateProfile profile);
        Task UpdateCandidateProfileAsync(CandidateProfile updatedProfile);
        Task ReplaceWorkExperiencesAsync(Guid candidateId, IEnumerable<CandidateWorkExperience> workExperiences);
        Task<CandidateWorkExperience> AddWorkExperienceAsync(CandidateWorkExperience workExperience);
        Task UpdateWorkExperienceAsync(CandidateWorkExperience workExperience);
        Task DeleteWorkExperienceAsync(Guid workExperienceId);
        Task ReplaceCertificatesAsync(Guid candidateId, IEnumerable<CandidateCertificate> certificates);
        Task<CandidateCertificate> AddCandidateCertificateAsync(CandidateCertificate certificate);
        Task UpdateCandidateCertificateAsync(CandidateCertificate certificate);
        Task DeleteCandidateCertificateAsync(Guid certificateId);
        void DeleteCandidateProfile(Guid id);
    }
}
