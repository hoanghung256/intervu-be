﻿using Intervu.Application.DTOs.User;
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
                return Ok(new { 
                    success = false,
                    message = "Invalid email or password"
                });
            }
            
            return Ok(new {
                success = true,
                data = response
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
    }
}
