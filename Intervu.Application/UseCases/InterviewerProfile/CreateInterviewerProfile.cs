using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Interviewer;
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
            await _repo.CreateInterviewerProfile(interviewerCreateDto);
            return _mapper.Map<InterviewerProfileDto>(interviewerCreateDto);
        }
    }
}
