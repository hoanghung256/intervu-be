using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.InterviewRoom
{
    public class CreateRoomReportRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ExpectTo { get; set; }
    }

    // Backward-compatible payload used by existing FE modal
    public class CreateRoomReportLegacyRequest
    {
        public Guid InterviewRoomId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ExpectTo { get; set; }
    }

    public class CreateRoomReportResult
    {
        public Guid ReportId { get; set; }
    }

    public class InterviewReportFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public InterviewReportStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public Guid? ReporterId { get; set; }
    }

    public class InterviewReportItemDto
    {
        public Guid Id { get; set; }
        public Guid InterviewRoomId { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ExpectTo { get; set; }
        public InterviewReportStatus Status { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class InterviewRoomReportDetailDto
    {
        public Guid ReportId { get; set; }
        public Guid InterviewRoomId { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? ExpectTo { get; set; }
        public InterviewReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public RoomReportBookingContextDto BookingContext { get; set; } = new();
        public RoomReportFinancialStatusDto FinancialStatus { get; set; } = new();
    }

    public class RoomReportBookingContextDto
    {
        public string? CoachName { get; set; }
        public string? CandidateName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime? OriginalTime { get; set; }
        public string? CandidateBankBinNumber { get; set; }
        public string? CandidateBankAccountNumber { get; set; }
    }

    public class RoomReportFinancialStatusDto
    {
        public string? PaymentStatus { get; set; }
        public string? PayOsOrderCode { get; set; }
        public bool PayoutLocked { get; set; }
    }
}
