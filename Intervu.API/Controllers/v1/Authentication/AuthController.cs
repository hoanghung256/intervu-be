using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Application.DTOs.PasswordReset;
using Intervu.Application.Interfaces.UseCases.Authentication;
using Intervu.Application.DTOs.User;

namespace Intervu.API.Controllers.v1.Authentication
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IGoogleLoginUseCase _googleLoginUseCase;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IForgotPasswordUseCase _forgotPasswordUseCase;
        private readonly IValidateResetTokenUseCase _validateResetTokenUseCase;
        private readonly IResetPasswordUseCase _resetPasswordUseCase;

        public AuthController(
            IGoogleLoginUseCase googleLoginUseCase,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IForgotPasswordUseCase forgotPasswordUseCase,
            IValidateResetTokenUseCase validateResetTokenUseCase,
            IResetPasswordUseCase resetPasswordUseCase)
        {
            _googleLoginUseCase = googleLoginUseCase;
            _configuration = configuration;
            _logger = logger;
            _forgotPasswordUseCase = forgotPasswordUseCase;
            _validateResetTokenUseCase = validateResetTokenUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
        }

        [HttpPost("google")]
        [AllowAnonymous]
        [Consumes("application/json", "application/x-www-form-urlencoded")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest? body, [FromForm] GoogleLoginRequest? form)
        {
            // Try to get token from JSON body first, then from form, then directly from Request.Form
            var rawToken = body?.IdToken ?? body?.Credential ?? form?.IdToken ?? form?.Credential;
            if (string.IsNullOrEmpty(rawToken) && Request.HasFormContentType)
            {
                var f = Request.Form;
                rawToken = f["credential"].FirstOrDefault()
                           ?? f["idToken"].FirstOrDefault()
                           ?? f["id_token"].FirstOrDefault()
                           ?? f["token"].FirstOrDefault();
            }

            var response = await _googleLoginUseCase.ExecuteAsync(rawToken ?? string.Empty);

            SetRefreshTokenCookie(response.RefreshToken);

            return Ok(new
            {
                success = true,
                message = "Logged in",
                data = new
                {
                    user = response.User,
                    token = response.Token,
                    expiresIn = response.ExpiresIn
                }
            });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var validationError = ValidateModelState();
            if (validationError != null)
            {
                return validationError;
            }

            var result = await _forgotPasswordUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                expiresAt = result.ExpiresAt
            });
        }

        [HttpGet("validate-reset-token/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateResetToken(string token)
        {
            if(string.IsNullOrEmpty(token))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Token is required"
                });
            }

            var result = await _validateResetTokenUseCase.ExecuteAsync(new ValidateResetTokenRequest { Token = token });

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                expiresAt = result.ExpiresAt
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var validationError = ValidateModelState();
            if (validationError != null)
            {
                return validationError;
            }

            var result = await _resetPasswordUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        private IActionResult? ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Invalid request";

                return BadRequest(new
                {
                    success = false,
                    message = firstError,
                    expiresAt = (DateTime?)null
                });
            }

            return null;
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            int expiryDays = _configuration.GetValue<int>("JwtConfig:RefreshTokenValidityInDays");
            if (expiryDays <= 0) expiryDays = 7;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(expiryDays)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
