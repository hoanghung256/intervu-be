using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Intervu.Application.Services
{
    public class JwtService
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IConfiguration _configuration;
        public JwtService(ILoginUseCase loginUseCase, IConfiguration configuration)
        {
            _loginUseCase = loginUseCase;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Authenticate(LoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return null;
            }

            var user = await _loginUseCase.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password);

            if (user == null || !PasswordHashHandler.VerifyPassword(loginRequest.Password, user.Password))
            {
                return null;
            }

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = _configuration["Jwt:Key"];
            var tokenValidityMins = _configuration.GetValue<int>("Jwt:TokenValidityInMinutes");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Name, loginRequest.Email)
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return new LoginResponse
            {
                Email = loginRequest.Email,
                Token = accessToken,
                ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds
            };
        }
    }
}
