using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class CoachWorkExperience : EntityBase<Guid>
    {
        public Guid CoachProfileId { get; set; }

        public CoachProfile? CoachProfile { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        // Position or title
        public string PositionTitle { get; set; } = string.Empty;

        // Job type (e.g., Full-time, Internship, Contract)
        public string? JobType { get; set; }

        // Location text (city, country)
        public string? Location { get; set; }

        // Location type (e.g., Remote, On-site, Hybrid)
        public string? LocationType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentWorking { get; set; }

        public bool IsEnded { get; set; }

        public string? Description { get; set; }

        public List<Guid> SkillIds { get; set; } = new();
    }
}
