using Intervu.Application.DTOs.BookingRequest;

namespace Intervu.Application.Interfaces.UseCases.BookingRequest
{
    public interface IGetBookingRequests
    {
        /// <summary>
        /// Returns paged booking requests for a user (candidate or coach), with optional filters.
        /// </summary>
        Task<(IReadOnlyList<BookingRequestDto> Items, int TotalCount)> ExecuteAsync(
            Guid userId, bool isCoach, GetBookingRequestsFilterDto filter);
    }
}
