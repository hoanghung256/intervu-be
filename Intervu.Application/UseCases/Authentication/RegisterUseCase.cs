using AutoMapper;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.Utils;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace Intervu.Application.UseCases.Authentication
{
    public class RegisterUseCase : IRegisterUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IMapper _mapper;
        private readonly IBackgroundService _backgroundService;
        private readonly IConfiguration _configuration;

        public RegisterUseCase(
            IUserRepository userRepository,
            ICandidateProfileRepository candidateProfileRepository,
            IMapper mapper,
            IBackgroundService backgroundService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _mapper = mapper;
            _backgroundService = backgroundService;
            _configuration = configuration;
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

            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var welcomePlaceholders = new Dictionary<string, string>
            {
                ["FullName"] = user.FullName,
                ["LoginLink"] = $"{frontendUrl.TrimEnd('/')}/login"
            };

            _backgroundService.Enqueue<IEmailService>(svc => svc.SendEmailWithTemplateAsync(
                user.Email,
                "Welcome",
                welcomePlaceholders));

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
