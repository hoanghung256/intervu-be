using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.Utils;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Authentication
{
    public class RegisterUseCase : IRegisterUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IMapper _mapper;

        public RegisterUseCase(IUserRepository userRepository, ICandidateProfileRepository candidateProfileRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _mapper = mapper;
        }

        public async Task<bool> ExecuteAsync(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return false;
            }

            var emailExists = await _userRepository.EmailExistsAsync(request.Email);
            if (emailExists)
            {
                return false;
            }

            var user = _mapper.Map<User>(request);
            user.Password = PasswordHashHandler.HashPassword(request.Password);

            // Parse role from string to enum
            if (Enum.TryParse<UserRole>(request.Role, out var role))
            {
                user.Role = role;
            }
            else
            {
                user.Role = UserRole.Candidate; // Default role
            }

            user.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(request.FullName);

            // Set default active status
            user.Status = UserStatus.Active;

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Create CandidateProfile if role is Candidate
            if (user.Role == UserRole.Candidate)
            {
                var profile = new Domain.Entities.CandidateProfile
                {
                    // Shared PK with User
                    // Id is set by EF when adding, but for shared key we ensure FK equals User.Id
                    Id = user.Id,
                    CVUrl = "",
                    PortfolioUrl = "",
                    Skills = new List<Domain.Entities.Skill>(),
                    Bio = "",
                    CurrentAmount = 0
                };

                await _candidateProfileRepository.AddAsync(profile);
                await _candidateProfileRepository.SaveChangesAsync();
            }

            return true;
        }
    }
}
