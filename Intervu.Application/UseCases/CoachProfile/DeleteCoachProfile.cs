using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.CoachProfile
{
    public class DeleteCoachProfile : IDeleteCoachProfile
    {
        private readonly ICoachProfileRepository _repo;

        public DeleteCoachProfile(ICoachProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task ExecuteAsync(Guid id)
        {
            Domain.Entities.CoachProfile? profile = await _repo.GetProfileByIdAsync(id);
            if (profile == null)
                throw new Exception("Profile not found.");
            _repo.DeleteAsync(profile);
            await _repo.SaveChangesAsync();
        }
    }
}
