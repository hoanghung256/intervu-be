namespace Intervu.Domain.Entities.Constants
{
    public enum BookingRequestType
    {
        // Flow B — outside coach's available time
        // Legacy
        External,
        // Flow C — multi-round JD-based
        JDInterview,
        // Flow A — direct availability booking
        Direct
    }
}
