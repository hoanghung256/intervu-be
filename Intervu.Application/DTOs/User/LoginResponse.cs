namespace Intervu.Application.DTOs.User
{
    public class LoginResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }

        public LoginResponse() { }

        public LoginResponse(string Email, String Token, int ExpiresIn)
        {
            this.Email = Email;
            this.Token = Token;
            this.ExpiresIn = ExpiresIn;
        }
    }
}
