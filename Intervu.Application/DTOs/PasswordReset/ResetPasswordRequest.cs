using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.PasswordReset
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; }
        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
