namespace Intervu.Domain.Entities.Constants
{
    public enum BookingRequestStatus
    {
        Pending,    // Initial state after candidate submits request
        PendingForApprovalAfterPayment,    // Candidate has paid, waiting for coach's approval
        Accepted,   // Coach accepted — candidate can pay
        Rejected,   // Coach declined
        Expired,    // Timed out
        Cancelled   // Candidate withdrew
    }
}
