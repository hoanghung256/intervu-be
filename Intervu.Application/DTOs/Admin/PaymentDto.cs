using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Application.DTOs.Admin
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int InterviewRoomId { get; set; }
        public int IntervieweeId { get; set; }
        public int InterviewerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
