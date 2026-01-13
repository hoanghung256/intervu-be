using Intervu.Domain.Entities.Constants;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Coach
{
    public class CoachCreateDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public UserRole Role { get; set; } = UserRole.Coach;
        public string? ProfilePicture { get; set; }
        public UserStatus UserStatus { get; set; } = UserStatus.Active;

        public int? CurrentAmount { get; set; }
        public int? ExperienceYears { get; set; }
        public CoachProfileStatus Status { get; set; } = CoachProfileStatus.Enable;

        public List<Guid> CompanyIds { get; set; } = new List<Guid>();
        public List<Guid> SkillIds { get; set; } = new List<Guid>();
    }
}
