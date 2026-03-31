namespace Intervu.Domain.Entities
{
    /// <summary>
    /// Represents a blocked sub-range inside an availability window.
    /// </summary>
    public record BlockedTime
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Reason { get; set; }
    }
}
