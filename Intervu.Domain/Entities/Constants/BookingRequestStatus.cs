namespace Intervu.Domain.Entities.Constants
{
    public enum BookingRequestStatus
    {
        Pending,    // Awaiting coach decision (Flow B) or awaiting payment (Flow C)
        Accepted,   // Coach accepted — candidate can pay
        Rejected,   // Coach declined
        Paid,       // Payment completed — rooms will be created
        Expired,    // Timed out
        Cancelled   // Candidate withdrew
    }
}
