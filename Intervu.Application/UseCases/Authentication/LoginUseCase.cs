using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Services;

namespace Intervu.Application.UseCases.Authentication
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;

        public LoginUseCase(
            IUserRepository userRepository, 
            IMapper mapper, 
            JwtService jwtService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse?> ExecuteAsync(LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return null;
            }

            // Get user from repository
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Verify user exists and password is correct
            if (user == null || !PasswordHashHandler.VerifyPassword(request.Password, user.Password))
            {
                return null;
            }

            // Generate JWT token using JwtService
            var token = _jwtService.GenerateToken(
                user.Id.ToString(),
                user.Email,
                user.Role.ToString()
            );

            // Get token expiry time
            var expiresIn = _jwtService.GetTokenValidityInSeconds();

            // Detach password before returning user data
            user.Password = null;

            return new LoginResponse
            {
                User = user,
                Token = token,
                ExpiresIn = expiresIn
            };
        }
    }
}
