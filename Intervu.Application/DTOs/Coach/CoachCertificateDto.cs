namespace Intervu.Application.DTOs.Coach
{
    public class CoachCertificateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiryAt { get; set; }
        public string? Link { get; set; }
    }
}
