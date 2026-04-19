using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Admin
{
    public class AdminCreateUserDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(300)]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(300)]
        public string? Email { get; set; }

        [MinLength(8)]
        [MaxLength(128)]
        public string? Password { get; set; }
        public UserRole Role { get; set; }

        [Url]
        [MaxLength(1000)]
        public string? ProfilePicture { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
