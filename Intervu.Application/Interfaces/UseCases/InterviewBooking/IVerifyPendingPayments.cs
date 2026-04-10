namespace Intervu.Application.Interfaces.UseCases.InterviewBooking
{
    /// <summary>
    /// Reconciles pending payment transactions with PayOS.
    /// Returns the count of transactions confirmed as paid in this run.
    /// Called by PaymentVerificationJob every 5 minutes.
    /// </summary>
    public interface IVerifyPendingPayments
    {
        Task<int> ExecuteAsync();
    }
}
