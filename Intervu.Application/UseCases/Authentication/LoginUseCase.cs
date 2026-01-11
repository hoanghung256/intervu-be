using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Utils;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Authentication
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginUseCase(
            IUserRepository userRepository, 
            IMapper mapper, 
            JwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponse?> ExecuteAsync(LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return null;
            }

            // Get user from repository
            User? user = await _userRepository.GetByEmailAsync(request.Email);

            // Verify user exists and password is correct
            if (user == null || !PasswordHashHandler.VerifyPassword(request.Password, user.Password))
            {
                return null;
            }

            // Generate JWT token using JwtService
            var token = _jwtService.GenerateToken(
                user.Id,
                user.Email,
                user.Role.ToString()
            );

            // Get token expiry time
            var expiresIn = _jwtService.GetTokenValidityInSeconds();

            // Revoke all existing refresh tokens for this user (logout from all devices)
            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

            // Generate new Refresh Token
            var refreshToken = await _refreshTokenRepository.CreateRefreshTokenAsync(user.Id);

            // Detach password before returning user data
            user.Password = null;

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
