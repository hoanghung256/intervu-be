using AutoMapper;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Utils;

namespace Intervu.Application.UseCases.CoachProfile
{
    public class UpdateCoachProfile : IUpdateCoachProfile
    {
        private readonly ICoachProfileRepository _repo;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public UpdateCoachProfile(ICoachProfileRepository repo, ICompanyRepository companyRepository, ISkillRepository skillRepository, IMapper mapper)
        {
            _repo = repo;
            _companyRepository = companyRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<CoachProfileDto> ExecuteAsync(Guid id, CoachUpdateDto coachUpdateDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Coach profile not found.");

            existing.User.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(coachUpdateDto.FullName);

            // Map Companies by IDs (DTO provides List<Guid> Companies)
            if (coachUpdateDto.CompanyIds != null)
            {
                var companies = await _companyRepository.GetByIdsAsync(coachUpdateDto.CompanyIds);
                existing.Companies = companies.ToList();
            }

            // Map Skills by IDs (DTO provides List<Guid> Skills)
            if (coachUpdateDto.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(coachUpdateDto.SkillIds);
                existing.Skills = skills.ToList();
            }

            // Map simple properties from DTO to existing entity
            _mapper.Map(coachUpdateDto, existing);

            await _repo.UpdateCoachProfileAsync(existing);

            await _repo.SaveChangesAsync();

            return _mapper.Map<CoachProfileDto>(existing);
        }

        public async Task<CoachProfileDto> UpdateCoachStatus(Guid id, CoachProfileStatus status)
        {
            Domain.Entities.CoachProfile? profile = await _repo.GetByIdAsync(id);
            if (profile == null)
                throw new Exception("Profile not found!");
            profile.Status = status;
            await _repo.SaveChangesAsync();
            return _mapper.Map<CoachProfileDto>(profile);
        }
    }
}
