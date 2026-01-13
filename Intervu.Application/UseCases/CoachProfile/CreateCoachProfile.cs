using AutoMapper;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.CoachProfile
{
    public class CreateCoachProfile : ICreateCoachProfile
    {
        private readonly ICoachProfileRepository _repo;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public CreateCoachProfile(ICoachProfileRepository repo, ICompanyRepository companyRepository, ISkillRepository skillRepository, IMapper mapper)
        {
            _repo = repo;
            _companyRepository = companyRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<CoachProfileDto> CreateCoachRequest(CoachCreateDto coachCreateDto)
        {
            var profile = _mapper.Map<Domain.Entities.CoachProfile>(coachCreateDto);

            //Add new Coach User
            profile.User = new User
            {
                FullName = coachCreateDto.FullName,
                Email = coachCreateDto.Email,
                Password = PasswordHashHandler.HashPassword(coachCreateDto.Password),
                Role = coachCreateDto.Role,
                ProfilePicture = coachCreateDto.ProfilePicture,
                Status = coachCreateDto.UserStatus,
                SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(coachCreateDto.FullName)
            };

            profile.Companies = (await _companyRepository.GetByIdsAsync(coachCreateDto.CompanyIds)).ToList();

            profile.Skills = (await _skillRepository.GetByIdsAsync(coachCreateDto.SkillIds)).ToList();

            await _repo.CreateCoachProfileAsync(profile);
            return _mapper.Map<CoachProfileDto>(profile);
        }
    }
}
