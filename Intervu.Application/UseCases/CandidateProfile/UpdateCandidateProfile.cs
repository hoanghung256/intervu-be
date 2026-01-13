using AutoMapper;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CandidateProfile
{
    public class UpdateCandidateProfile : IUpdateCandidateProfile
    {
        private readonly ICandidateProfileRepository _repo;
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;

        public UpdateCandidateProfile(ICandidateProfileRepository repo, ISkillRepository skillRepository, IMapper mapper)
        {
            _repo = repo;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<CandidateProfileDto> UpdateCandidateProfileAsync(Guid id, CandidateUpdateDto updateDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Candidate profile not found.");

            existing.User.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(updateDto.FullName);

            // Map Skills by IDs
            if (updateDto.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(updateDto.SkillIds);
                existing.Skills = skills.ToList();
            }

            // Map simple properties from DTO to existing entity
            _mapper.Map(updateDto, existing);

            await _repo.UpdateCandidateProfileAsync(existing);
            await _repo.SaveChangesAsync();

            return _mapper.Map<CandidateProfileDto>(existing);
        }

        public async Task<CandidateViewDto> UpdateCandidateStatusAsync(Guid id, UserStatus status)
        {
            var profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Candidate profile not found!");

            profile.User.Status = status;
            await _repo.SaveChangesAsync();

            return _mapper.Map<CandidateViewDto>(profile);
         }

        async Task<Domain.Entities.CandidateProfile> IUpdateCandidateProfile.UpdateCandidateCVProfile(Guid id, string cvUrl)
        {
            Domain.Entities.CandidateProfile profile = await _repo.GetByIdAsync(id);
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
