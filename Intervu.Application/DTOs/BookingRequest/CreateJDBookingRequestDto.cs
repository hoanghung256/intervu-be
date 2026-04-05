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
        [MinLength(1, ErrorMessage = "At least 1 round is required")]
        public List<CreateInterviewRoundDto> Rounds { get; set; } = [];
    }

    /// <summary>
    /// A single round in the JD multi-round booking request.
    /// Each round references consecutive 30-min CoachAvailability blocks.
    /// </summary>
    public class CreateInterviewRoundDto
    {
        /// <summary>
        /// Which CoachInterviewService this round uses (determines type, price, duration)
        /// </summary>
        [Required]
        public Guid CoachInterviewServiceId { get; set; }

        /// <summary>
        /// The IDs of consecutive 30-min CoachAvailability blocks for this round.
        /// The number of blocks must match Service.DurationMinutes / 30.
        /// Blocks must be strictly consecutive (Block[n].EndTime == Block[n+1].StartTime).
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one availability block is required per round")]
        public List<Guid> AvailabilityIds { get; set; } = [];
    }
}
