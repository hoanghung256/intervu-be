using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.RescheduleRequest
{
    public class CreateRescheduleRequestDto
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public Guid ProposedAvailabilityId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;
    }
}
