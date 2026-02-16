using AutoMapper;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using DomainCandidateProfile = Intervu.Domain.Entities.CandidateProfile;
using DomainCoachProfile = Intervu.Domain.Entities.CoachProfile;
using DomainCompany = Intervu.Domain.Entities.Company;
using DomainSkill = Intervu.Domain.Entities.Skill;

namespace Intervu.Application.UseCases.Admin
{
    public class CreateUserForAdmin : ICreateUserForAdmin
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IMapper _mapper;

        public CreateUserForAdmin(
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

        public async Task<AdminUserResponseDto> ExecuteAsync(AdminCreateUserDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException($"Email '{dto.Email}' is registered");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = PasswordHashHandler.HashPassword(dto.Password),
                Role = dto.Role,
                ProfilePicture = dto.ProfilePicture,
                Status = dto.Status,
                SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(dto.FullName)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            if (user.Role == UserRole.Candidate)
            {
                var profile = new DomainCandidateProfile
                {
                    Id = user.Id,
                    User = user,
                    CVUrl = string.Empty,
                    PortfolioUrl = string.Empty,
                    Skills = new List<DomainSkill>(),
                    Bio = string.Empty,
                    CurrentAmount = 0
                };

                await _candidateProfileRepository.AddAsync(profile);
                await _candidateProfileRepository.SaveChangesAsync();
            }
            else if (user.Role == UserRole.Coach)
            {
                var profile = new DomainCoachProfile
                {
                    Id = user.Id,
                    User = user,
                    PortfolioUrl = string.Empty,
                    CurrentAmount = 0,
                    ExperienceYears = 0,
                    Bio = string.Empty,
                    BankBinNumber = string.Empty,
                    BankAccountNumber = string.Empty,
                    Status = CoachProfileStatus.Enable,
                    Companies = new List<DomainCompany>(),
                    Skills = new List<DomainSkill>()
                };

                await _coachProfileRepository.AddAsync(profile);
                await _coachProfileRepository.SaveChangesAsync();
            }

            return _mapper.Map<AdminUserResponseDto>(user);
        }
    }
}
