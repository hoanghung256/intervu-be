using AutoMapper;
using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CoachProfile
{
    public class ViewCoachProfile : IViewCoachProfile
    {
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ViewCoachProfile(ICoachProfileRepository coachProfileRepository, IUserRepository userRepository, IMapper mapper)
        {
            _coachProfileRepository = coachProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CoachProfileDto?> ViewOwnProfileAsync(Guid id)
        {
            Domain.Entities.User? userData = await _userRepository.GetByIdAsync(id);
            if (userData == null) return null;

            Domain.Entities.CoachProfile? profileData = await _coachProfileRepository.GetProfileByIdAsync(id);
            if (profileData == null) return null;

            CoachProfileDto result = _mapper.Map<CoachProfileDto>(profileData);

            result.User = _mapper.Map<UserDto>(userData);

            return result;
        }

        public async Task<CoachViewDto?> ViewProfileForCandidateAsync(string slug)
        {
            Domain.Entities.User? userData = await _userRepository.GetBySlugAsync(slug);
            if (userData == null) return null;

            Domain.Entities.CoachProfile? profile = await _coachProfileRepository.GetProfileBySlugAsync(slug);
            CoachViewDto result = _mapper.Map<CoachViewDto>(profile);
            result.User = _mapper.Map<UserDto>(userData);
            return result;
        }
    }
}
