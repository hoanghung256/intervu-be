using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Filter/pagination DTO for listing booking requests
    /// </summary>
    public class GetBookingRequestsFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Filter by request type (External, JDInterview)
        /// </summary>
        public BookingRequestType? Type { get; set; }

        /// <summary>
        /// Filter by status(es)
        /// </summary>
        public List<BookingRequestStatus>? Statuses { get; set; }
    }
}
