using Intervu.Application.DTOs.User;
using Intervu.Application.DTOs.Skill;

namespace Intervu.Application.DTOs.Interviewee
{
    public class IntervieweeViewDto
    {
        public Guid Id { get; set; }
        public UserDto User { get; set; } = null!;
        public string? PortfolioUrl { get; set; }
        public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
        public string? Bio { get; set; }
    }
}
