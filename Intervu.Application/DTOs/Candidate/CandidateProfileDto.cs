using Intervu.Application.DTOs.Skill;
using Intervu.Application.DTOs.User;

namespace Intervu.Application.DTOs.Candidate
{
    public class CandidateProfileDto
    {
        public Guid Id { get; set; }
        public UserDto User { get; set; } = null!;
        public string? CVUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
        public string? Bio { get; set; }
        public int CurrentAmount { get; set; }
    }
}
