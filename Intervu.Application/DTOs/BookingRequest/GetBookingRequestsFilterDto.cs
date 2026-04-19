using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.BookingRequest
{
    /// <summary>
    /// Filter/pagination DTO for listing booking requests
    /// </summary>
    public class GetBookingRequestsFilterDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
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
