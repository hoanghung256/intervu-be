using Intervu.Application.DTOs.User;
using Intervu.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController
    {
        private readonly JwtService _jwtService;
        public AccountController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var response = await _jwtService.Authenticate(loginRequest);
            if (response == null)
            {
                return null;
            }
            return response;
        }
    }
}
