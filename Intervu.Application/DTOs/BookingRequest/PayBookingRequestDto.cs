using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Request DTO for paying an Accepted booking request
    /// </summary>
    public class PayBookingRequestDto
    {
        [Required]
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
