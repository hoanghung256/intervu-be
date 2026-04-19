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
