using Intervu.Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Asp.Versioning;
using FirebaseAdmin.Messaging;

namespace Intervu.API.Controllers.v1.Authentication
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IRegisterUseCase _registerUseCase;
        private readonly IRefreshTokenUseCase _refreshTokenUseCase;
        private readonly IConfiguration _configuration;

        public AccountController(ILoginUseCase loginUseCase, IRegisterUseCase registerUseCase, IRefreshTokenUseCase refreshTokenUseCase, IConfiguration configuration)
        {
            _loginUseCase = loginUseCase;
            _registerUseCase = registerUseCase;
            _refreshTokenUseCase = refreshTokenUseCase;
            _configuration = configuration; 
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _loginUseCase.ExecuteAsync(loginRequest);
            
            if (response == null)
            {
                return Ok(new { 
                    success = false,
                    message = "Invalid email or password"
                });
            }

            SetRefreshTokenCookie(response.RefreshToken);

            return Ok(new {
                success = true,
                data = new {
                    user = response.User,
                    token = response.Token,
                    expiresIn = response.ExpiresIn
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]  
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var success = await _registerUseCase.ExecuteAsync(registerRequest);
            
            if (!success)
            {
                return BadRequest(new { message = "Registration failed. Email may already exist." });
            }
            
            return Ok(new { message = "Registration successful" });
        }
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Refresh token not found"
                });
            }

            var response = await _refreshTokenUseCase.ExecuteAsync(new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            });

            if (response == null)
            {
                Response.Cookies.Delete("refreshToken");

                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or expired refresh token"
                });
            }

            SetRefreshTokenCookie(response.RefreshToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    accessToken = response.AccessToken,
                    expiresIn = response.ExpiresIn
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            Response.Cookies.Append("refreshToken", "", cookieOptions);

            return Ok(new
            {
                success = true,
                message = "Logged out successfully"
            });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            int expiryDays = _configuration.GetValue<int>("JwtConfig:RefreshTokenValidityInDays");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(expiryDays)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
