namespace Intervu.Application.DTOs.Availability
{
    public class BlockCoachAvailabilityTimeDto
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Reason { get; set; }
    }
}
