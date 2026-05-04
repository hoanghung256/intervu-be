using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.InterviewBooking
{
    public class InterviewBookingRequest : IValidatableObject
    {
        [Required]
        public Guid CoachId { get; set; }

        [Required]
        public Guid CoachAvailabilityId { get; set; }

        [Required]
        public Guid CoachInterviewServiceId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [Url]
        public string ReturnUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional: roadmap node skill_id this booking targets — propagated to InterviewRoom
        /// so the post-interview update can deterministically update the originating node.
        /// </summary>
        public string? RoadmapNodeId { get; set; }

        /// <summary>Optional note for the coach (max 1000 characters).</summary>
        [StringLength(1000)]
        public string? CandidateNote { get; set; }

        /// <summary>
        /// Required when the selected interview type has <c>RequiresCandidateCv</c>; otherwise optional.
        /// </summary>
        public string? CVUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CoachId == Guid.Empty)
            {
                yield return new ValidationResult("CoachId is required.", [nameof(CoachId)]);
            }

            if (CoachAvailabilityId == Guid.Empty)
            {
                yield return new ValidationResult("CoachAvailabilityId is required.", [nameof(CoachAvailabilityId)]);
            }

            if (CoachInterviewServiceId == Guid.Empty)
            {
                yield return new ValidationResult("CoachInterviewServiceId is required.", [nameof(CoachInterviewServiceId)]);
            }

            if (StartTime <= DateTime.UtcNow)
            {
                yield return new ValidationResult("StartTime must be in the future.", [nameof(StartTime)]);
            }
        }
    }
}
