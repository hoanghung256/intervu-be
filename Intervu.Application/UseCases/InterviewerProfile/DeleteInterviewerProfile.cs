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

        public Task<bool> DeleteInterviewerProfileAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteInterviewProfile(int id)
        {
            var profile = _repo.GetByIdAsync(id);
            if (profile == null)
                return;
            _repo.DeleteInterviewerProfile(id);
        }
    }
}
