using AutoMapper;
using Intervu.Application.DTOs.Interviewee;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.IntervieweeProfile
{
    public class ViewIntervieweeProfile : IViewIntervieweeProfile
    {
        private readonly IIntervieweeProfileRepository _intervieweeProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ViewIntervieweeProfile(
            IIntervieweeProfileRepository intervieweeProfileRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _intervieweeProfileRepository = intervieweeProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IntervieweeProfileDto?> ViewOwnProfileAsync(Guid id)
        {
            var userData = await _userRepository.GetByIdAsync(id);
            if (userData == null) return null;

            var profileData = await _intervieweeProfileRepository.GetProfileByIdAsync(id);
            if (profileData == null) return null;

            var result = _mapper.Map<IntervieweeProfileDto>(profileData);
            result.User = _mapper.Map<UserDto>(userData);

            return result;
        }

        public async Task<IntervieweeViewDto?> ViewProfileBySlugAsync(string slug)
        {
            var userData = await _userRepository.GetBySlugAsync(slug);
            if (userData == null) return null;

            var profile = await _intervieweeProfileRepository.GetProfileBySlugAsync(slug);
            if (profile == null) return null;

            var result = _mapper.Map<IntervieweeViewDto>(profile);
            result.User = _mapper.Map<UserDto>(userData);

            return result;
        }
    }
}
