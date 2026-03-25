using Intervu.Application.DTOs.User;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Intervu.Application.UseCases.Authentication
{
    public class GoogleLoginUseCase : IGoogleLoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtService _jwtService;
        private readonly IGoogleTokenValidator _googleTokenValidator;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public GoogleLoginUseCase(
            IUserRepository userRepository,
            ICandidateProfileRepository candidateProfileRepository,
            IRefreshTokenRepository refreshTokenRepository,
            JwtService jwtService,
            IGoogleTokenValidator googleTokenValidator,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _userRepository = userRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _googleTokenValidator = googleTokenValidator;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<LoginResponse> ExecuteAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new BadRequestException("IdToken (or credential) is required");
            }

            var configuredClientId = _configuration["Google:ClientId"];
            var enforceAudience = !_environment.IsDevelopment() && !string.IsNullOrWhiteSpace(configuredClientId);

            var payload = await _googleTokenValidator.ValidateAsync(idToken, configuredClientId, enforceAudience);

            if (!payload.EmailVerified)
            {
                throw new BadRequestException("Google account email not verified");
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new BadRequestException("Google account email is missing");
            }

            var user = await _userRepository.GetByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new User
                {
                    FullName = string.IsNullOrWhiteSpace(payload.Name)
                        ? payload.Email.Split('@')[0]
                        : payload.Name,
                    Email = payload.Email,
                    Password = PasswordHashHandler.HashPassword(Guid.NewGuid().ToString()),
                    Role = UserRole.Candidate,
                    Status = UserStatus.Active,
                    ProfilePicture = payload.Picture,
                    SlugProfileUrl = SlugProfileUrlHandler.GenerateProfileSlug(string.IsNullOrWhiteSpace(payload.Name)
                        ? payload.Email.Split('@')[0]
                        : payload.Name)
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                var profile = new Domain.Entities.CandidateProfile
                {
                    Id = user.Id,
                    CVUrl = string.Empty,
                    PortfolioUrl = string.Empty,
                    Skills = new List<Domain.Entities.Skill>(),
                    Bio = string.Empty,
                    CurrentAmount = 0
                };

                await _candidateProfileRepository.AddAsync(profile);
                await _candidateProfileRepository.SaveChangesAsync();
            }

            if (user.Status != UserStatus.Active)
            {
                throw new ForbiddenException("Account is not active");
            }

            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());
            var expiresIn = _jwtService.GetTokenValidityInSeconds();
            var refreshToken = await _refreshTokenRepository.CreateRefreshTokenAsync(user.Id);

            user.Password = null!;

            return new LoginResponse
            {
                User = user,
                Token = token,
                ExpiresIn = expiresIn,
                RefreshToken = refreshToken
            };
        }
    }
}