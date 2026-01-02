using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.Utils;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Authentication
{
     public class RefreshTokenUseCase : IRefreshTokenUseCase
     {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtService _jwtService;

        public RefreshTokenUseCase(IRefreshTokenRepository refreshTokenRepository, JwtService jwtService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
        }
        public async Task<RefreshTokenResponse?> ExecuteAsync(RefreshTokenRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return null;
            }

            var existingToken = await _refreshTokenRepository.GetValidTokenAsync(request.RefreshToken);

            if(existingToken == null)
            {
                return null;
            }

            var user = existingToken.User;
            var newJwtToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

            var newRefreshToken = await _refreshTokenRepository.CreateRefreshTokenAsync(user.Id);

            await _refreshTokenRepository.RevokeTokenAsync(request.RefreshToken);

            var expiresIn = _jwtService.GetTokenValidityInSeconds();

            return new RefreshTokenResponse
            {
                AccessToken = newJwtToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn
            };
        }
    }
}
