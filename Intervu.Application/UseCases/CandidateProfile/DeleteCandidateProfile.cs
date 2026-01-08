using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CandidateProfile
{
    public class DeleteCandidateProfile : IDeleteCandidateProfile
    {
        private readonly ICandidateProfileRepository _repo;

        public DeleteCandidateProfile(ICandidateProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task DeleteCandidateProfileAsync(Guid id)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Candidate profile not found.");

            _repo.DeleteCandidateProfile(id);
            await _repo.SaveChangesAsync();
        }
    }
}
