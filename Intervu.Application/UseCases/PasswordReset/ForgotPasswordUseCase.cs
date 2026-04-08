using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.PasswordReset;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Intervu.Application.UseCases.PasswordReset
{
    public class ForgotPasswordUseCase : IForgotPasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ForgotPasswordUseCase(
            IUserRepository userRepository,
            IPasswordResetTokenRepository tokenRepository,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _emailService = emailService;
            _configuration = configuration;
        }
        public async Task<PasswordResetResponse> ExecuteAsync(ForgotPasswordRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                return new PasswordResetResponse
                {
                    Success = true,
                    Message = "If the email is registered, a password reset link has been sent."
                };
            }

            //Invalidate existing tokens if exist
            await _tokenRepository.InvalidateAllUserTokensAsync(user.Id);

            var token = GenerateSecureToken();

            var expiresAt = DateTime.UtcNow.AddHours(24);

            await _tokenRepository.CreateTokenAsync(user.Id, token, expiresAt);

            var frontEndUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontEndUrl}/reset-password?token={token}";

            var placeholders = new Dictionary<string, string>
            {
                ["FullName"] = user.FullName,
                ["ResetLink"] = resetLink,
                ["ExpiryHours"] = "24"
            };

            // Send email synchronously so user gets immediate feedback if sending fails
            try
            {
                await _emailService.SendEmailWithTemplateAsync(user.Email, "ForgotPassword", placeholders);
            }
            catch (Exception)
            {
                // Log the exception (not implemented here for brevity)
                return new PasswordResetResponse
                {
                    Success = false,
                    Message = "Failed to send password reset email. Please try again later."
                };
            }

            return new PasswordResetResponse
            {
                Success = true,
                Message = "Password reset link has been sent to your email.",
                ExpiresAt = expiresAt
            };

        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

    }
}
