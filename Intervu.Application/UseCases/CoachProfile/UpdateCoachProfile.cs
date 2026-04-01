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
        private readonly IIndustryRepository _industryRepository;
        private readonly IMapper _mapper;

        public UpdateCoachProfile(ICoachProfileRepository repo, ICompanyRepository companyRepository, ISkillRepository skillRepository, IIndustryRepository industryRepository, IMapper mapper)
        {
            _repo = repo;
            _companyRepository = companyRepository;
            _skillRepository = skillRepository;
            _industryRepository = industryRepository;
            _mapper = mapper;
        }

        public async Task<CoachProfileDto> ExecuteAsync(Guid id, CoachUpdateDto dto)
        {
            var existing = await _repo.GetProfileByIdAsync(id);
            if (existing == null)
                throw new Exception("Coach profile not found.");

            if (existing.User != null)
            {
                existing.User.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(dto.FullName);
            }

            if (dto.CompanyIds != null)
            {
                var companies = await _companyRepository.GetByIdsAsync(dto.CompanyIds);
                existing.Companies.Clear();
                foreach (var c in companies)
                    existing.Companies.Add(c);
            }

            if (dto.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(dto.SkillIds);
                existing.Skills.Clear();
                foreach (var s in skills)
                    existing.Skills.Add(s);
            }

            if (dto.IndustryIds != null)
            {
                var industries = await _industryRepository.GetByIdsAsync(dto.IndustryIds);
                existing.Industries.Clear();
                foreach (var i in industries)
                    existing.Industries.Add(i);
            }

            _mapper.Map(dto, existing);

            await _repo.UpdateCoachProfileAsync(existing);

            var reloaded = await _repo.GetProfileByIdAsync(id);
            return _mapper.Map<CoachProfileDto>(reloaded!);
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
