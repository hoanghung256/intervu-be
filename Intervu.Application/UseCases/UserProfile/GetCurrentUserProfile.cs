using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.UserProfile
{
    public class GetCurrentUserProfile : IGetCurrentUserProfile
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IMapper _mapper;

        public GetCurrentUserProfile(
            IUserRepository userRepository,
            ICandidateProfileRepository candidateProfileRepository,
            ICoachProfileRepository coachProfileRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _coachProfileRepository = coachProfileRepository;
            _mapper = mapper;
        }

        public async Task<CurrentUserProfileDto?> ExecuteAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var result = new CurrentUserProfileDto
            {
                User = _mapper.Map<UserDto>(user),
                Role = user.Role,
            };

            if (user.Role == UserRole.Candidate)
            {
                var candidateProfile = await _candidateProfileRepository.GetProfileByIdAsync(userId);
                if (candidateProfile != null)
                {
                    result.CandidateProfile = new CurrentUserCandidateProfileDto
                    {
                        Id = candidateProfile.Id,
                        BankBinNumber = candidateProfile.BankBinNumber,
                        // Keep response safe: always expose masked form, never ciphertext.
                        BankAccountNumber = candidateProfile.BankAccountNumberMasked,
                    };
                }
            }

            if (user.Role == UserRole.Coach)
            {
                var coachProfile = await _coachProfileRepository.GetProfileByIdAsync(userId);
                if (coachProfile != null)
                {
                    result.CoachProfile = new CurrentUserCoachProfileDto
                    {
                        Id = coachProfile.Id,
                        BankBinNumber = coachProfile.BankBinNumber,
                        // Keep response safe: always expose masked form, never ciphertext.
                        BankAccountNumber = coachProfile.BankAccountNumberMasked,
                    };
                }
            }

            return result;
        }
    }
}