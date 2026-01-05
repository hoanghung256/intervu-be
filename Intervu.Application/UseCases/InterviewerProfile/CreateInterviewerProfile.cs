using AutoMapper;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class CreateInterviewerProfile : ICreateInterviewerProfile
    {
        private readonly IInterviewerProfileRepository _repo;
        private readonly ICompanyRepository _companyRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public CreateInterviewerProfile(IInterviewerProfileRepository repo, ICompanyRepository companyRepository, ISkillRepository skillRepository, IMapper mapper)
        {
            _repo = repo;
            _companyRepository = companyRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<InterviewerProfileDto> CreateInterviewerRequest(InterviewerCreateDto interviewerCreateDto)
        {
            var profile = _mapper.Map<Domain.Entities.InterviewerProfile>(interviewerCreateDto);

            //Add new Interviewer User
            profile.User = new User
            {
                FullName = interviewerCreateDto.FullName,
                Email = interviewerCreateDto.Email,
                Password = interviewerCreateDto.Password,
                Role = interviewerCreateDto.Role,
                ProfilePicture = interviewerCreateDto.ProfilePicture,
                Status = interviewerCreateDto.UserStatus,
                SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(interviewerCreateDto.FullName)
            };

            profile.Companies = (await _companyRepository.GetByIdsAsync(interviewerCreateDto.CompanyIds)).ToList();

            profile.Skills = (await _skillRepository.GetByIdsAsync(interviewerCreateDto.SkillIds)).ToList();

            await _repo.CreateInterviewerProfileAsync(profile);
            return _mapper.Map<InterviewerProfileDto>(profile);
        }
    }
}
