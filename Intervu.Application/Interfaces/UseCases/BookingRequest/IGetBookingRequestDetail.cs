using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface IGetBookingRequestDetail
    {
        /// <summary>
        /// Returns a single booking request with all navigation details.
        /// Validates that the caller is the candidate or coach involved.
        /// </summary>
        Task<BookingRequestDto> ExecuteAsync(Guid userId, Guid bookingRequestId);
    }
}
