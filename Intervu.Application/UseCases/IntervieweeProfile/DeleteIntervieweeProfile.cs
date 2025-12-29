using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.IntervieweeProfile
{
    public class DeleteIntervieweeProfile : IDeleteIntervieweeProfile
    {
        private readonly IIntervieweeProfileRepository _repo;

        public DeleteIntervieweeProfile(IIntervieweeProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task DeleteIntervieweeProfileAsync(Guid id)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Interviewee profile not found.");

            _repo.DeleteIntervieweeProfile(id);
            await _repo.SaveChangesAsync();
        }
    }
}
