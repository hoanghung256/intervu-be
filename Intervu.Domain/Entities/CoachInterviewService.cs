using Intervu.Domain.Abstractions.Entity;

namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Many-to-Many with payload: links a Coach to the InterviewTypes they offer,
    /// with coach-specific custom price and duration.
    /// Unique constraint: (CoachId, InterviewTypeId)
    /// </summary>
    public class CoachInterviewService : EntityBase<Guid>
    {
        public Guid CoachId { get; set; }

        public Guid InterviewTypeId { get; set; }

        /// <summary>
        /// Coach's custom price — must be within InterviewType.MinPrice..MaxPrice
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Coach's custom duration in minutes
        /// </summary>
        public int DurationMinutes { get; set; }

        // Navigation
        public CoachProfile CoachProfile { get; set; } = null!;
        public InterviewType InterviewType { get; set; } = null!;
    }
}
