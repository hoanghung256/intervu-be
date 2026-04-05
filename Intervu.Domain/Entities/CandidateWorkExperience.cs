using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class CandidateWorkExperience : EntityBase<Guid>
    {
        public Guid CandidateProfileId { get; set; }

        public CandidateProfile? CandidateProfile { get; set; }

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
