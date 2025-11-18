using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Application.DTOs.Admin
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string? ProfilePicture { get; set; }
        public UserStatus Status { get; set; }
    }
}
