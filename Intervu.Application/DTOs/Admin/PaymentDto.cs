using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Application.DTOs.Admin
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid InterviewRoomId { get; set; }
        public Guid CandidateId { get; set; }
        public Guid InterviewerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
