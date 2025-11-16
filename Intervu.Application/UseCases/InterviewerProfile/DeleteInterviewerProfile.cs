using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class DeleteInterviewerProfile : IDeleteInterviewerProfile
    {
        private readonly IInterviewerProfileRepository _repo;

        public DeleteInterviewerProfile(IInterviewerProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task DeleteInterviewProfile(int id)
        {
            Domain.Entities.InterviewerProfile? profile = await _repo.GetProfileByIdAsync(id);
            if (profile == null)
                throw new Exception("Profile not found.");
            _repo.DeleteAsync(profile);
            await _repo.SaveChangesAsync();
        }
    }
}
