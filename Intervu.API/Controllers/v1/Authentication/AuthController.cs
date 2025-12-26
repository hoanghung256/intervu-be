using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Asp.Versioning;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Intervu.Domain.Repositories;
using Intervu.Application.Utils;
using Intervu.Application.Interfaces.UseCases.PasswordReset;
using Intervu.Application.DTOs.PasswordReset;

namespace Intervu.API.Controllers.v1.Authentication
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IForgotPasswordUseCase _forgotPasswordUseCase;
        private readonly IValidateResetTokenUseCase _validateResetTokenUseCase;
        private readonly IResetPasswordUseCase _resetPasswordUseCase;

        public AuthController(IUserRepository userRepository, JwtService jwtService, IConfiguration configuration, ILogger<AuthController> logger, IForgotPasswordUseCase forgotPasswordUseCase, IValidateResetTokenUseCase validateResetTokenUseCase, IResetPasswordUseCase resetPasswordUseCase)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
            _forgotPasswordUseCase = forgotPasswordUseCase;
            _validateResetTokenUseCase = validateResetTokenUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
        }

        // Accept either { "idToken": "..." } or { "credential": "..." }
        public class GoogleLoginRequest
        {
            public string? IdToken { get; set; }
            public string? credential { get; set; }
        }

        [HttpPost("google")]
        [AllowAnonymous]
        [Consumes("application/json", "application/x-www-form-urlencoded")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest? body, [FromForm] GoogleLoginRequest? form)
        {
            var contentType = Request.ContentType;

            // Try to get token from JSON body first, then from form, then directly from Request.Form
            var rawToken = body?.IdToken ?? body?.credential ?? form?.IdToken ?? form?.credential;
            if (string.IsNullOrEmpty(rawToken) && Request.HasFormContentType)
            {
                var f = Request.Form;
                rawToken = f["credential"].FirstOrDefault()
                           ?? f["idToken"].FirstOrDefault()
                           ?? f["id_token"].FirstOrDefault()
                           ?? f["token"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(rawToken))
            {
                _logger.LogWarning("Google login missing token. ContentType: {ContentType}", contentType);
                return BadRequest(new { success = false, message = "IdToken (or credential) is required", contentType });
            }

            // Validate ID token using Google.Apis.Auth
            var configuredClientId = _configuration["Google:ClientId"];
            var envName = _configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
            var isDevelopment = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings();
                // In Development, skip audience enforcement to ease local testing
                if (!isDevelopment && !string.IsNullOrEmpty(configuredClientId))
                    settings.Audience = new[] { configuredClientId };
                payload = await GoogleJsonWebSignature.ValidateAsync(rawToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token. Env: {Env}, ClientId: {ClientId}", envName, configuredClientId);
                return BadRequest(new { success = false, message = "Invalid Google ID token", detail = ex.Message, env = envName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google token validation failed");
                return BadRequest(new { success = false, message = "Google token validation failed", detail = ex.Message });
            }

            var email = payload.Email;
            var name = payload.Name;
            var picture = payload.Picture;
            // optional: require email_verified == true
            if (payload.EmailVerified == false)
            {
                return BadRequest(new { success = false, message = "Google account email not verified" });
            }

            // find or create user
            var user = await _userRepository.GetByEmailAsync(email!);
            if (user == null)
            {
                user = new User
                {
                    FullName = string.IsNullOrEmpty(name) ? email!.Split('@')[0] : name!,
                    Email = email!,
                    Password = PasswordHashHandler.HashPassword(Guid.NewGuid().ToString()),
                    Role = UserRole.Interviewee,
                    Status = UserStatus.Active,
                    ProfilePicture = picture
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }

            // generate jwt
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString());
            var expiresIn = _jwtService.GetTokenValidityInSeconds();

            user.Password = null!;

            return Ok(new { success = true, message = "Logged in", data = new { user, token, expiresIn } });
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

        [HttpPost("validate-reset-token/{token}")]
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
    }
}
