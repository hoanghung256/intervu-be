using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.Utils;
using Intervu.Domain.Repositories;
using System.Net.Mail;

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

        public async Task<Intervu.Application.DTOs.User.RegisterResult> ExecuteAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new Intervu.Application.DTOs.User.RegisterResult { Success = false, Message = "Email is required" };
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new Intervu.Application.DTOs.User.RegisterResult { Success = false, Message = "Password is required" };
            }

            if (!IsValidEmail(request.Email))
            {
                return new Intervu.Application.DTOs.User.RegisterResult { Success = false, Message = "Invalid email format" };
            }

            if (request.Password.Length < 8)
            {
                return new Intervu.Application.DTOs.User.RegisterResult { Success = false, Message = "Password must be at least 8 characters" };
            }

            var emailExists = await _userRepository.EmailExistsAsync(request.Email);
            if (emailExists)
            {
                return new Intervu.Application.DTOs.User.RegisterResult { Success = false, Message = "Email already exists" };
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

            return new Intervu.Application.DTOs.User.RegisterResult { Success = true, Message = "Registration successful" };
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
