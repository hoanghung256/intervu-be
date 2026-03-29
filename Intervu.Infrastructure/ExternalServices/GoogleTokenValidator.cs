using Google.Apis.Auth;
using Intervu.Application.DTOs.User;
using Intervu.Application.Exceptions;
using Intervu.Application.Interfaces.ExternalServices;

namespace Intervu.Infrastructure.ExternalServices
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        public async Task<GoogleUserInfo> ValidateAsync(string idToken, string? clientId, bool enforceAudience)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings();
                if (enforceAudience && !string.IsNullOrWhiteSpace(clientId))
                {
                    settings.Audience = new[] { clientId };
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new GoogleUserInfo
                {
                    Email = payload.Email ?? string.Empty,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }
            catch (InvalidJwtException)
            {
                throw new BadRequestException("Invalid Google ID token");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch
            {
                throw new BadRequestException("Google token validation failed");
            }
        }
    }
}