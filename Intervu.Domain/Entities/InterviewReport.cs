using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class InterviewReport
    {
        public Guid Id { get; set; }
        public Guid InterviewRoomId { get; set; }
        public InterviewRoom? InterviewRoom { get; set; }
        public Guid ReportedBy { get; set; }
        public Guid? ReporterId { get; set; }
        public User? Reporter { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ExpectTo { get; set; }
        public InterviewReportStatus Status { get; set; } = InterviewReportStatus.Pending;
        public string? AdminNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
