using Intervu.Domain.Entities.Constants;
using System.Collections.Generic;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerCreateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; } = UserRole.Interviewer;
        public string? ProfilePicture { get; set; }
        public UserStatus UserStatus { get; set; } = UserStatus.Active;

        public int? CurrentAmount { get; set; }
        public int? ExperienceYears { get; set; }
        public InterviewerProfileStatus Status { get; set; } = InterviewerProfileStatus.Enable;

        public List<Guid> CompanyIds { get; set; } = new List<Guid>();
        public List<Guid> SkillIds { get; set; } = new List<Guid>();
    }
}
