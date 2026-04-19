using Intervu.Domain.Entities.Constants;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.InterviewBooking
{
    public class GetInterviewBookingHistoryRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 10;

        public TransactionType? Type { get; set; }

        public TransactionStatus? Status { get; set; }
    }
}
