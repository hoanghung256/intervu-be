using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.User
{
    public class UpdateProfileRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(300)]
        public string FullName { get; set; } = string.Empty;
    }
}
