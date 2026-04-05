namespace Intervu.Application.DTOs.Coach
{
    public class CoachWorkExperienceDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string? JobType { get; set; }
        public string? Location { get; set; }
        public string? LocationType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrentWorking { get; set; }
        public bool IsEnded { get; set; }
        public string? Description { get; set; }
        public List<Guid> SkillIds { get; set; } = new();
    }
}
