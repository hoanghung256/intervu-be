using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.InterviewBooking
{
    public class GetInterviewBookingHistoryRequest
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public TransactionType? Type { get; set; }

        public TransactionStatus? Status { get; set; }
    }
}
