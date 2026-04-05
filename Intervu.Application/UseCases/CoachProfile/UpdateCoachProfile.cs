using AutoMapper;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<CoachProfileDto> ExecuteAsync(Guid id, CoachUpdateDto request)
        {
            var existingProfile = await _repo.GetProfileByIdAsync(id);
            if (existingProfile == null)
                throw new Exception("Coach profile not found.");

            _mapper.Map(request, existingProfile);
            existingProfile.Id = id; 

            if (request.SkillIds != null)
            {
                var skills = await _skillRepository.GetByIdsAsync(request.SkillIds);
                existingProfile.Skills = skills.ToList();
            }

            if (request.CompanyIds != null)
            {
                var companies = await _companyRepository.GetByIdsAsync(request.CompanyIds);
                existingProfile.Companies = companies.ToList();
            }

            // Update name and slug from name
            if (request.FullName != null)
            {
                existingProfile.User.FullName = request.FullName;
                existingProfile.User.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(request.FullName);
            }

            await _repo.UpdateCoachProfileAsync(existingProfile);

            return _mapper.Map<CoachProfileDto>(existingProfile);
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
