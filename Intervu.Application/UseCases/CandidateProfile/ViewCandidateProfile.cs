using AutoMapper;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CandidateProfile
{
    public class ViewCandidateProfile : IViewCandidateProfile
    {
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ViewCandidateProfile(
            ICandidateProfileRepository candidateProfileRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _candidateProfileRepository = candidateProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CandidateProfileDto?> ViewOwnProfileAsync(Guid id)
        {
            var userData = await _userRepository.GetByIdAsync(id);
            if (userData == null) return null;

            var profileData = await _candidateProfileRepository.GetProfileByIdAsync(id);
            if (profileData == null) return null;

            var result = _mapper.Map<CandidateProfileDto>(profileData);
            result.User = _mapper.Map<UserDto>(userData);

            return result;
        }

        public async Task<CandidateViewDto?> ViewProfileBySlugAsync(string slug)
        {
            var userData = await _userRepository.GetBySlugAsync(slug);
            if (userData == null) return null;

            var profile = await _candidateProfileRepository.GetProfileBySlugAsync(slug);
            if (profile == null) return null;

            var result = _mapper.Map<CandidateViewDto>(profile);
            result.User = _mapper.Map<UserDto>(userData);

            return result;
        }
    }
}
