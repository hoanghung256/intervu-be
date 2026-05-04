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

        [Url]
        [MaxLength(1000)]
        public string? PortfolioUrl { get; set; }

        public int? CurrentAmount { get; set; }

        [Range(0, 80)]
        public int? ExperienceYears { get; set; }

        [MaxLength(200)]
        public string? CurrentJobTitle { get; set; }

        [MaxLength(4000)]
        public string? Bio { get; set; }
    }
}
