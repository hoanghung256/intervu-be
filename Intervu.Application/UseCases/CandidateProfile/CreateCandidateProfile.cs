using AutoMapper;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.CandidateProfile
{
    public class CreateCandidateProfile : ICreateCandidateProfile
    {
        private readonly ICandidateProfileRepository _repo;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;

        public CreateCandidateProfile(ICandidateProfileRepository repo, ISkillRepository skillRepository, IUserRepository userRepository, IMapper mapper)
        {
            _repo = repo;
            _skillRepository = skillRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CandidateProfileDto> CreateCandidateProfileAsync(CandidateCreateDto candidateCreateDto)
        {
            var profile = _mapper.Map<Domain.Entities.CandidateProfile>(candidateCreateDto);

            // Link to already-registered account by UserId
            var existingUser = await _userRepository.GetByIdAsync(candidateCreateDto.UserId);
            if (existingUser == null)
            {
                throw new InvalidOperationException("Account not found. Please register first.");
            }

            profile.Id = existingUser.Id;
            profile.User = existingUser;

            if (candidateCreateDto.SkillIds != null && candidateCreateDto.SkillIds.Count > 0)
            {
                profile.Skills = (await _skillRepository.GetByIdsAsync(candidateCreateDto.SkillIds)).ToList();
            }

            await _repo.CreateCandidateProfileAsync(profile);
            return _mapper.Map<CandidateProfileDto>(profile);
        }
    }
}
