namespace Intervu.Application.DTOs.Candidate
{
    public class CandidateCreateDto
    {
        // Link to the already-registered user; no need to re-enter account info
        public Guid UserId { get; set; }

        // Profile-specific fields
        public string? CVUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public List<Guid>? SkillIds { get; set; }
        public string? Bio { get; set; }
    }
}
