using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.User
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(300)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(300)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Candidate"; // Default role

        [MaxLength(255)]
        public string SlugProfileUrl { get; set; } = string.Empty;
    }
}
