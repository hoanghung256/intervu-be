using Intervu.Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intervu.Application.Interfaces.UseCases.Authentication;

namespace Intervu.API.Controllers.Authentication
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IRegisterUseCase _registerUseCase;
        
        public AccountController(ILoginUseCase loginUseCase, IRegisterUseCase registerUseCase)
        {
            _loginUseCase = loginUseCase;
            _registerUseCase = registerUseCase;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _loginUseCase.ExecuteAsync(loginRequest);
            
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
            
            return Ok(response);
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
    }
}
