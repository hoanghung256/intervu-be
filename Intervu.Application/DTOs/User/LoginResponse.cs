namespace Intervu.Application.DTOs.User
{
    public class LoginResponse
    {
        public Domain.Entities.User User { get; set; }
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }

        public LoginResponse() { }

        public LoginResponse(Domain.Entities.User user, String Token, int ExpiresIn)
        {
            this.User = user;
            this.Token = Token;
            this.ExpiresIn = ExpiresIn;
        }
    }
}
