using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Request DTO for Flow C: JD Multi-Round Interview Booking
    /// Candidate submits a JD + CV for a multi-round interview plan
    /// </summary>
    public class CreateJDBookingRequestDto
    {
        [Required]
        public Guid CoachId { get; set; }

        /// <summary>
        /// URL of the uploaded Job Description file
        /// </summary>
        [Required]
        [Url]
        public string JobDescriptionUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL of the CV — either newly uploaded or picked from CandidateProfile
        /// </summary>
        [Required]
        [Url]
        public string CVUrl { get; set; } = string.Empty;

        /// <summary>
        /// Target interview level
        /// </summary>
        public AimLevel? AimLevel { get; set; }

        /// <summary>
        /// The interview rounds the candidate wants.
        /// Validation:
        /// - At least 2 rounds required
        /// - Round[n+1].StartTime >= Round[n].EndTime + 15 minutes
        /// - Each round must reference a valid CoachInterviewService belonging to the coach
        /// </summary>
        [Required]
        [MinLength(2, ErrorMessage = "At least 2 rounds are required for JD multi-round interviews")]
        public List<CreateInterviewRoundDto> Rounds { get; set; } = [];
    }

    /// <summary>
    /// A single round in the JD multi-round booking request
    /// </summary>
    public class CreateInterviewRoundDto
    {
        /// <summary>
        /// Which CoachInterviewService this round uses (determines type, price, duration)
        /// </summary>
        [Required]
        public Guid CoachInterviewServiceId { get; set; }

        /// <summary>
        /// Scheduled start time for this round
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }
    }
}
