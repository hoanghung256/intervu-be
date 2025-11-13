using AutoMapper;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;

namespace Intervu.Application.UseCases.InterviewerProfile
{
    public class ViewInterviewerProfile : IViewInterviewProfile
    {
        private readonly IInterviewerProfileRepository _interviewerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ViewInterviewerProfile(IInterviewerProfileRepository interviewerProfileRepository, IUserRepository userRepository, IMapper mapper)
        {
            _interviewerProfileRepository = interviewerProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<InterviewerProfileDto?> ViewOwnProfileAsync(int id)
        {
            Domain.Entities.User? userData = await _userRepository.GetByIdAsync(id);
            if (userData == null) return null;

            Domain.Entities.InterviewerProfile? profileData = await _interviewerProfileRepository.GetProfileByIdAsync(id);
            if (profileData == null) return null;

            InterviewerProfileDto result = new InterviewerProfileDto
            {
                Id = profileData.Id,
                User = _mapper.Map<UserDto>(userData),
                CVUrl = profileData.CVUrl,
                PortfolioUrl = profileData.PortfolioUrl,
                Companies = _mapper.Map<List<CompanyDto>>(profileData.Companies),
                Skills = _mapper.Map<List<SkillDto>>(profileData.Skills),
            };

            return result;
        }

        public async Task<InterviewerViewDto?> ViewProfileForIntervieweeAsync(int id)
        {
            var profile = await _interviewerProfileRepository.GetByIdAsync(id);
            return profile != null ? _mapper.Map<InterviewerViewDto>(profile) : throw new Exception("Profile not found!");
        }
    }
}
