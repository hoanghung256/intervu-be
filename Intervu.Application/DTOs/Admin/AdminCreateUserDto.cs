using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Admin
{
    public class AdminCreateUserDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public UserRole Role { get; set; }
        public string? ProfilePicture { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
