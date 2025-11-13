using Intervu.Application.DTOs.Company;
using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Interviewer
{
    public class InterviewerProfileDto
    {
        public int Id { get; set; }

        public UserDto User { get; set; }

        public string CVUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public int CurrentAmount { get; set; }

        public int ExperienceYears { get; set; }

        public string Bio { get; set; }

        public InterviewerProfileStatus Status { get; set; }

        public ICollection<CompanyDto> Companies { get; set; } = new List<CompanyDto>();

        public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
    }
}
