using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class CoachCertificate : EntityBase<Guid>
    {
        public Guid CoachProfileId { get; set; }

        public CoachProfile? CoachProfile { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiryAt { get; set; }
        public string? Link { get; set; }
    }
}
