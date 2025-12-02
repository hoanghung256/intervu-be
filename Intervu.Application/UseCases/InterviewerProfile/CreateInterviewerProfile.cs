using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class CreateInterviewerProfile : ICreateInterviewProfile
    {
        private readonly IInterviewerProfileRepository _repo;
        private readonly IMapper _mapper;

        public CreateInterviewerProfile(IInterviewerProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InterviewerProfileDto> CreateInterviewRequest(InterviewerCreateDto interviewerCreateDto)
        {
            var profile = _mapper.Map<Domain.Entities.InterviewerProfile>(interviewerCreateDto);
            await _repo.CreateInterviewerProfile(profile);
            return _mapper.Map<InterviewerProfileDto>(profile);
        }
    }
}
