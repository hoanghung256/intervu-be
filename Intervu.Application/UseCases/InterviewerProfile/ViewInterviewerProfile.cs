using AutoMapper;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Repositories;

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

        public async Task<InterviewerProfileDto?> ViewOwnProfileAsync(Guid id)
        {
            Domain.Entities.User? userData = await _userRepository.GetByIdAsync(id);
            if (userData == null) return null;

            Domain.Entities.InterviewerProfile? profileData = await _interviewerProfileRepository.GetProfileByIdAsync(id);
            if (profileData == null) return null;

            InterviewerProfileDto result = _mapper.Map<InterviewerProfileDto>(profileData);

            result.User = _mapper.Map<UserDto>(userData);

            return result;
        }

        public async Task<InterviewerViewDto?> ViewProfileForIntervieweeAsync(Guid id)
        {
            Domain.Entities.User? userData = await _userRepository.GetByIdAsync(id);
            if (userData == null) return null;

            Domain.Entities.InterviewerProfile? profile = await _interviewerProfileRepository.GetProfileByIdAsync(id);
            InterviewerViewDto result = _mapper.Map<InterviewerViewDto>(profile);
            result.User = _mapper.Map<UserDto>(userData);
            return result;
        }
    }
}
