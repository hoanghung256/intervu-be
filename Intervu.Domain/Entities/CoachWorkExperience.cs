using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class CoachWorkExperience : EntityBase<Guid>
    {
        public Guid CoachProfileId { get; set; }

        public CoachProfile? CoachProfile { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentWorking { get; set; }

        public bool IsEnded { get; set; }

        public string? Description { get; set; }

        public List<Guid> SkillIds { get; set; } = new();
    }
}
