using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    public class CandidateCertificate : EntityBase<Guid>
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile? CandidateProfile { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiryAt { get; set; }
        public string? Link { get; set; }
    }
}
