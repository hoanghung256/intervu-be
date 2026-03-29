using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.BookingRequest
{
    public class RescheduleJDBookingRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one round must be selected for reschedule")]
        public List<RescheduleJDRoundDto> Rounds { get; set; } = [];
    }

    public class RescheduleJDRoundDto
    {
        [Required]
        public Guid InterviewRoomId { get; set; }

        [Required]
        public DateTime NewStartTime { get; set; }
    }
}
