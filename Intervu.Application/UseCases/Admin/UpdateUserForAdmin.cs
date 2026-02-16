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
    public class UpdateUserForAdmin : IUpdateUserForAdmin
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IMapper _mapper;

        public UpdateUserForAdmin(
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

        public async Task<AdminUserResponseDto> ExecuteAsync(Guid userId, AdminCreateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User v?i ID {userId} kh�ng t?n t?i.");

            var previousRole = user.Role;

            // Ki?m tra n?u email ?ang ???c thay ??i v� ?� t?n t?i
            if (user.Email != dto.Email && await _userRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException($"Email '{dto.Email}' ?� t?n t?i.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.ProfilePicture = dto.ProfilePicture;
            user.Status = dto.Status;

            _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            if (previousRole != user.Role)
            {
                if (previousRole == UserRole.Candidate)
                {
                    var existingCandidate = await _candidateProfileRepository.GetProfileByIdAsync(user.Id);
                    if (existingCandidate != null)
                    {
                        _candidateProfileRepository.DeleteCandidateProfile(user.Id);
                        await _candidateProfileRepository.SaveChangesAsync();
                    }
                }
                else if (previousRole == UserRole.Coach)
                {
                    var existingCoach = await _coachProfileRepository.GetProfileByIdAsync(user.Id);
                    if (existingCoach != null)
                    {
                        _coachProfileRepository.DeleteCoachProfile(user.Id);
                        await _coachProfileRepository.SaveChangesAsync();
                    }
                }
            }

            if (user.Role == UserRole.Candidate)
            {
                var existingCandidate = await _candidateProfileRepository.GetProfileByIdAsync(user.Id);
                if (existingCandidate == null)
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
            }
            else if (user.Role == UserRole.Coach)
            {
                var existingCoach = await _coachProfileRepository.GetProfileByIdAsync(user.Id);
                if (existingCoach == null)
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
            }

            return _mapper.Map<AdminUserResponseDto>(user);
        }
    }
}
