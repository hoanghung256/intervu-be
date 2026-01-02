using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.PasswordReset;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.PasswordReset
{
    public class ValidateResetTokenUseCase : IValidateResetTokenUseCase
    {
        private readonly IPasswordResetTokenRepository _tokenRepository;

        public ValidateResetTokenUseCase(IPasswordResetTokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        public async Task<PasswordResetResponse> ExecuteAsync(ValidateResetTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "Token is required."
                };
            }

            var token = await _tokenRepository.GetValidTokenAsync(request.Token);

            if (token == null)
            {
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "Invalid or expired token."
                };
            }

            return new PasswordResetResponse
            {
                Success = true,
                Message = "Token is valid.",
                ExpiresAt = token.ExpiresAt
            };
        }
    }
}
