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
        private readonly IIntervieweeProfileRepository _intervieweeProfileRepository;
        private readonly IMapper _mapper;

        public RegisterUseCase(IUserRepository userRepository, IIntervieweeProfileRepository intervieweeProfileRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _intervieweeProfileRepository = intervieweeProfileRepository;
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
                user.Role = UserRole.Interviewee; // Default role
            }

            user.SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(request.FullName);

            // Set default active status
            user.Status = UserStatus.Active;

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Create IntervieweeProfile if role is Interviewee
            if (user.Role == UserRole.Interviewee)
            {
                var profile = new Domain.Entities.IntervieweeProfile
                {
                    // Shared PK with User
                    // Id is set by EF when adding, but for shared key we ensure FK equals User.Id
                    Id = user.Id,
                    CVUrl = "",
                    PortfolioUrl = "",
                    Skills = "[]",
                    Bio = "",
                    CurrentAmount = 0
                };

                await _intervieweeProfileRepository.AddAsync(profile);
                await _intervieweeProfileRepository.SaveChangesAsync();
            }

            return true;
        }
    }
}
