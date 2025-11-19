using Intervu.Application.Interfaces.Repositories;
using Intervu.Application.Services;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Asp.Versioning;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserRepository userRepository, JwtService jwtService, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
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
            catch (System.Exception ex)
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
                    Password = Intervu.Application.Services.PasswordHashHandler.HashPassword(System.Guid.NewGuid().ToString()),
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
    }
}
