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
        public UserStatus Status { get; set; } = UserStatus.Active;

        public int? CurrentAmount { get; set; }
        public int? ExperienceYears { get; set; }
        public InterviewerProfileStatus StatusProfile { get; set; } = InterviewerProfileStatus.Enable;

        public List<int> CompanyIds { get; set; } = new List<int>();
        public List<int> SkillIds { get; set; } = new List<int>();
    }
}
