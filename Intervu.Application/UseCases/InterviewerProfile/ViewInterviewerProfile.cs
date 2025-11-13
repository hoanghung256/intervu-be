using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class ViewInterviewerProfile :IViewInterviewProfile
    {
        private readonly IInterviewerProfileRepository _repo;
        private readonly IMapper _mapper;

        public ViewInterviewerProfile(IInterviewerProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InterviewerProfileDto> ViewOwnProfileAsync(int id)
        {
            var profile = await _repo.GetByIdAsync(id);
            return profile != null ? _mapper.Map<InterviewerProfileDto>(profile) : throw new Exception("Profile not found!");
        }

        public async Task<InterviewerViewDto> ViewProfileForIntervieweeAsync(int id)
        {
            var profile = await _repo.GetByIdAsync(id);
            return profile != null ? _mapper.Map<InterviewerViewDto>(profile) : throw new Exception("Profile not found!");
        }
    }
}
