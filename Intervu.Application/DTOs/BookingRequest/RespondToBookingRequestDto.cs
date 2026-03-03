using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Request DTO for coach to accept or reject a booking request
    /// </summary>
    public class RespondToBookingRequestDto
    {
        [Required]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Required when IsApproved = false
        /// </summary>
        [StringLength(500)]
        public string? RejectionReason { get; set; }
    }
}
