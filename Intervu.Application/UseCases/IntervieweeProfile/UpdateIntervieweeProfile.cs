using AutoMapper;
using Intervu.Application.DTOs.Interviewee;
using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities.Constants;
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
        private readonly ISkillRepository _skillRepository;

        public UpdateIntervieweeProfile(IIntervieweeProfileRepository repo, ISkillRepository skillRepository, IMapper mapper)
        {
            _repo = repo;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<IntervieweeProfileDto> UpdateIntervieweeProfileAsync(Guid id, IntervieweeUpdateDto updateDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Interviewee profile not found.");

            existing.User.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(updateDto.FullName);

            // Map Skills by IDs
            if (updateDto.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(updateDto.SkillIds);
                existing.Skills = skills.ToList();
            }

            // Map simple properties from DTO to existing entity
            _mapper.Map(updateDto, existing);

            await _repo.UpdateIntervieweeProfileAsync(existing);
            await _repo.SaveChangesAsync();

            return _mapper.Map<IntervieweeProfileDto>(existing);
        }

        public async Task<IntervieweeViewDto> UpdateIntervieweeStatusAsync(Guid id, UserStatus status)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Interviewee profile not found!");

            profile.User.Status = status;
            await _repo.SaveChangesAsync();

            return _mapper.Map<IntervieweeViewDto>(profile);
        }

        async Task<Domain.Entities.IntervieweeProfile> IUpdateIntervieweeProfile.UpdateIntervieweeCVProfile(Guid id, string cvUrl)
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
