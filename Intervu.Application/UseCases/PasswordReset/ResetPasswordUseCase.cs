using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.PasswordReset;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Application.Utils;
using Intervu.Domain.Repositories;
using Microsoft.VisualBasic;

namespace Intervu.Application.UseCases.PasswordReset
{
    public class ResetPasswordUseCase : IResetPasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository;

        public ResetPasswordUseCase(
            IUserRepository userRepository,
            IPasswordResetTokenRepository tokenRepository)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
        }
        public async Task<PasswordResetResponse> ExecuteAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "Token is required."
                };
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "New password is required."
                };
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "Passwords do not match."
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

            var user = await _userRepository.GetByIdAsync(token.UserId);

            if (user == null)
            {
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var hashedPassword = PasswordHashHandler.HashPassword(request.NewPassword);

            user.Password = hashedPassword;
            _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _tokenRepository.MarkAsUsedAsync(token.Id);

            await _tokenRepository.InvalidateAllUserTokensAsync(user.Id);

            return new PasswordResetResponse
            {
                Success = true,
                Message = "Password has been reset successfully. You can now login with your new password."
            };
        }
    }
}
