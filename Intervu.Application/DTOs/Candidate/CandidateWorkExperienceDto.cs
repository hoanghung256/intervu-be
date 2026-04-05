namespace Intervu.Application.DTOs.Candidate
{
    public class CandidateWorkExperienceDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrentWorking { get; set; }
        public bool IsEnded { get; set; }
        public string? Description { get; set; }
        public List<Guid> SkillIds { get; set; } = new();
    }
}
