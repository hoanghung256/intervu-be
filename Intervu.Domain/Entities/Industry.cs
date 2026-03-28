using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class Industry : EntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public ICollection<CoachProfile> CoachProfiles { get; set; } = new List<CoachProfile>();
    }
}
