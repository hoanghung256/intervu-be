using AutoMapper;
using Intervu.Application.DTOs.Interviewee;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.IntervieweeProfile
{
    public class CreateIntervieweeProfile : ICreateIntervieweeProfile
    {
        private readonly IIntervieweeProfileRepository _repo;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;

        public CreateIntervieweeProfile(IIntervieweeProfileRepository repo, ISkillRepository skillRepository, IUserRepository userRepository, IMapper mapper)
        {
            _repo = repo;
            _skillRepository = skillRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IntervieweeProfileDto> CreateIntervieweeProfileAsync(IntervieweeCreateDto intervieweeCreateDto)
        {
            var profile = _mapper.Map<Domain.Entities.IntervieweeProfile>(intervieweeCreateDto);

            // Link to already-registered account by UserId
            var existingUser = await _userRepository.GetByIdAsync(intervieweeCreateDto.UserId);
            if (existingUser == null)
            {
                throw new InvalidOperationException("Account not found. Please register first.");
            }

            profile.Id = existingUser.Id;
            profile.User = existingUser;

            if (intervieweeCreateDto.SkillIds != null && intervieweeCreateDto.SkillIds.Count > 0)
            {
                profile.Skills = (await _skillRepository.GetByIdsAsync(intervieweeCreateDto.SkillIds)).ToList();
            }

            await _repo.CreateIntervieweeProfileAsync(profile);
            return _mapper.Map<IntervieweeProfileDto>(profile);
        }
    }
}
