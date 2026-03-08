using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface ICancelBookingRequest
    {
        /// <summary>
        /// Candidate cancels a booking request that is still Pending or Accepted.
        /// Transitions Status → Cancelled.
        /// </summary>
        Task<BookingRequestDto> ExecuteAsync(Guid candidateId, Guid bookingRequestId);
    }
}
