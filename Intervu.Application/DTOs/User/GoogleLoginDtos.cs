using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.User
{
    public class GoogleLoginRequest
    {
        [MaxLength(5000)]
        public string? IdToken { get; set; }

        [MaxLength(5000)]
        public string? Credential { get; set; }
    }

    public class GoogleUserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public bool EmailVerified { get; set; }
    }
}
