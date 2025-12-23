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
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

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

            var frontEndUrl = request.FrontendUrl ?? _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:7118";
            var resetLink = $"{frontEndUrl}/reset-password?token={token}";

            //Send email
            try
            {
                await _emailService.SendEmailAsync(
                    to: user.Email,
                    subject: "Password Reset Request",
                    body: BuildEmailBody(user.FullName, resetLink, expiresAt),
                    isHtml: true
                );
            }
            catch (Exception ex)
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

        private string BuildEmailBody(string userName, string resetLink, DateTime expiresAt)
        {
            var expiryHours = (expiresAt - DateTime.UtcNow).TotalHours;

            return $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hi {userName},</p>
                    <p>You requested to reset your password. Click the link below to reset your password:</p>
                    <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{resetLink}</p>
                    <p><strong>This link will expire in {expiryHours:F0} hours.</strong></p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p>Best regards,<br/>Intervu Team</p>
                </body>
                </html>
            ";
        }
    }
}
