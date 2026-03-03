using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Request DTO for Flow B: External Booking
    /// Candidate requests a session outside the coach's available time ranges
    /// </summary>
    public class CreateExternalBookingRequestDto
    {
        [Required]
        public Guid CoachId { get; set; }

        [Required]
        public Guid CoachInterviewServiceId { get; set; }

        /// <summary>
        /// Desired start time (outside coach availability)
        /// </summary>
        [Required]
        public DateTime RequestedStartTime { get; set; }

        /// <summary>
        /// Target interview level
        /// </summary>
        public AimLevel? AimLevel { get; set; }
    }
}
