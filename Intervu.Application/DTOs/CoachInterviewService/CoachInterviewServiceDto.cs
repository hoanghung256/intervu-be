namespace Intervu.Application.DTOs.CoachInterviewService
{
    /// <summary>
    /// Response DTO for a coach's interview service offering
    /// </summary>
    public class CoachInterviewServiceDto
    {
        public Guid Id { get; set; }
        public Guid CoachId { get; set; }
        public Guid InterviewTypeId { get; set; }

        /// <summary>
        /// Interview type name (e.g., "System Design", "Behavioral")
        /// </summary>
        public string InterviewTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this service type includes coding exercises
        /// </summary>
        public bool IsCoding { get; set; }

        /// <summary>
        /// Coach's custom price for this service
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Coach's custom duration in minutes
        /// </summary>
        public int DurationMinutes { get; set; }
    }
}
