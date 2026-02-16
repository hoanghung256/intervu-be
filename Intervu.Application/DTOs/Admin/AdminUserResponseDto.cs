using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Admin
{
    public class AdminUserResponseDto
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public UserRole Role { get; set; }
        public string? ProfilePicture { get; set; }
        public string? SlugProfileUrl { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
