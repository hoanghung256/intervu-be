using System;

namespace Intervu.Application.DTOs.User
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
