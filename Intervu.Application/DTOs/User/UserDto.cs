using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// Interviewee, Interviewer, Admin
        /// </summary>
        public UserRole Role { get; set; }

        public string? ProfilePicture { get; set; }

        /// <summary>
        /// Active, Suspended, Deleted
        /// </summary>
        public UserStatus Status { get; set; }
    }
}
