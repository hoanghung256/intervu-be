using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class ResolveRoomReportRequest
    {
        public Guid ReportId { get; set; }
        public InterviewReportStatus Status { get; set; }
        public string? AdminNote { get; set; }
        public RefundOption? RefundOption { get; set; }
    }

    public enum RefundOption
    {
        None = 0,
        Partial50 = 50,
        Full100 = 100
    }
}
