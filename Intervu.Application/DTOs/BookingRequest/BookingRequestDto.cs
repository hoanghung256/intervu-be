using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Response DTO for a booking request
    /// </summary>
    public class BookingRequestDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string? CandidateName { get; set; }
        public Guid CoachId { get; set; }
        public string? CoachName { get; set; }

        public BookingRequestType Type { get; set; }
        public BookingRequestStatus Status { get; set; }

        // Service info
        public Guid? CoachInterviewServiceId { get; set; }
        public string? InterviewTypeName { get; set; }
        public int? ServicePrice { get; set; }
        public int? ServiceDurationMinutes { get; set; }

        // Flow B fields
        public DateTime? RequestedStartTime { get; set; }
        public AimLevel? AimLevel { get; set; }

        // Flow C fields
        public string? JobDescriptionUrl { get; set; }
        public string? CVUrl { get; set; }
        public List<InterviewRoundDto>? Rounds { get; set; }

        // Response
        public string? RejectionReason { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Payment
        public int TotalAmount { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// For Flow B: the single session ID and its status
        /// </summary>
        public Guid? InterviewRoomId { get; set; }
        public string? InterviewRoomStatus { get; set; }
    }

    /// <summary>
    /// Lightweight round info embedded in BookingRequestDto
    /// </summary>
    public class InterviewRoundDto
    {
        public Guid Id { get; set; }
        public int RoundNumber { get; set; }
        public Guid CoachInterviewServiceId { get; set; }
        public string? InterviewTypeName { get; set; }
        public bool IsCoding { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Price { get; set; }
        public Guid? InterviewRoomId { get; set; }
        public string? InterviewRoomStatus { get; set; }
        public string Status { get; set; } = "Active";
    }
}
