using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class UpdateInterviewerProfile : IUpdateInterviewProfile
    {
        private readonly IInterviewerProfileRepository _repo;
        private readonly IMapper _mapper;

        public UpdateInterviewerProfile(IInterviewerProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InterviewerProfileDto> UpdateInterviewProfile(int id, InterviewerUpdateDto interviewerUpdateDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return null;

            _mapper.Map(interviewerUpdateDto, existing);

            await _repo.UpdateInterviewerProfileAsync(existing);

            await _repo.SaveChangesAsync();

            return _mapper.Map<InterviewerProfileDto>(existing);
        }

        public async Task<InterviewerViewDto> UpdateInterviewStatus(int id, InterviewerProfileStatus status)
        {
            Domain.Entities.InterviewerProfile? profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Profile not found!");
            profile.Status = status;
            await _repo.SaveChangesAsync();
            return _mapper.Map<InterviewerViewDto>(profile);
        }
    }
}
