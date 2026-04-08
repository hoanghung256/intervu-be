namespace Intervu.Application.DTOs.Audit
{
    public class AuditLogItemDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Metadata { get; set; }
        public int EventType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
