namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface IExpireBookingRequests
    {
        /// <summary>
        /// Background job: Finds all pending booking requests that have passed their ExpiresAt
        /// and transitions them to Expired status.
        /// </summary>
        Task<int> ExecuteAsync();
    }
}
