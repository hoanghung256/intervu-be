using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.PasswordReset
{
    public class PasswordResetResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
