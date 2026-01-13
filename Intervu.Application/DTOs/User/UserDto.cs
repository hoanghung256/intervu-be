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
        public Guid Id { get; set; }
        public required string FullName { get; set; }

        public required string Email { get; set; }

        /// <summary>
        /// Candidate, Coach, Admin
        /// </summary>
        public UserRole Role { get; set; }

        public string? ProfilePicture { get; set; }
        public string? SlugProfileUrl { get; set; }

        /// <summary>
        /// Active, Suspended, Deleted
        /// </summary>
        public UserStatus Status { get; set; }
    }
}
