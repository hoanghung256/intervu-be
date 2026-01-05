using AutoMapper;
using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.IntervieweeProfile
{
    public class UpdateIntervieweeProfile : IUpdateIntervieweeProfile
    {
        private readonly IIntervieweeProfileRepository _repo;
        private readonly IMapper _mapper;

        public UpdateIntervieweeProfile(IIntervieweeProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        async Task<Domain.Entities.IntervieweeProfile> IUpdateIntervieweeProfile.UpdateIntervieweeProfile(Guid id, string cvUrl)
        {
            Domain.Entities.IntervieweeProfile profile = await _repo.GetByIdAsync(id);
            if (profile == null) {
                return null;
            }
            profile.CVUrl = cvUrl;

            _repo.UpdateAsync(profile);
            await _repo.SaveChangesAsync();

            return profile;
        }
    }
}
