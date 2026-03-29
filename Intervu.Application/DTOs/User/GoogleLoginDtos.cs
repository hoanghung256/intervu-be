namespace Intervu.Application.DTOs.User
{
    public class GoogleLoginRequest
    {
        public string? IdToken { get; set; }
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