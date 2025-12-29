using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.Interviewee
{
    public class IntervieweeUpdateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string? CVUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public List<Guid>? SkillIds { get; set; }
        public string? Bio { get; set; }
        public int CurrentAmount { get; set; }
        //public UserStatus UserStatus { get; set; }
    }
}
