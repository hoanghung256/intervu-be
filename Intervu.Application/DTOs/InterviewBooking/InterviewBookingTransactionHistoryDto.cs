using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.DTOs.InterviewBooking
{
    public class InterviewBookingTransactionHistoryDto
    {
        public Guid Id { get; set; }

        public int OrderCode { get; set; }

        public Guid UserId { get; set; }

        public Guid? CoachId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
