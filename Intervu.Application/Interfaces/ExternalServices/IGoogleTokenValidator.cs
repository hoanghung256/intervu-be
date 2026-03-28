using Intervu.Application.DTOs.User;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleUserInfo> ValidateAsync(string idToken, string? clientId, bool enforceAudience);
    }
}