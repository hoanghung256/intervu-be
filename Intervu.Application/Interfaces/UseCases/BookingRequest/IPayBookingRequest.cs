namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface IPayBookingRequest
    {
        /// <summary>
        /// Candidate pays for an Accepted booking request.
        /// Creates PayOS checkout URL and transactions.
        /// Returns the checkout URL (null if amount is 0).
        /// </summary>
        Task<string?> ExecuteAsync(Guid candidateId, Guid bookingRequestId, string returnUrl);
    }
}
