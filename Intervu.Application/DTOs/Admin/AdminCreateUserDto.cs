using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Admin
{
    public class AdminCreateUserDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; }
        public string? ProfilePicture { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
