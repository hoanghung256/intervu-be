namespace Intervu.Application.DTOs.User
{
    public class LoginResponse
    {
        public Domain.Entities.User User { get; set; }
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;    

        public LoginResponse() { }

        public LoginResponse(Domain.Entities.User user, string Token, string RefreshToken, int ExpiresIn)
        {
            this.User = user;
            this.Token = Token;
            this.RefreshToken = RefreshToken;
            this.ExpiresIn = ExpiresIn;
        }
    }
}
