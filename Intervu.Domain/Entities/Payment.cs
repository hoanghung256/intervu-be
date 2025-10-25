using Intervu.Domain.Abstractions.Entities;
using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Domain.Entities
{
    public class Payment : EntityBase<int>
    {
        /// <summary>
        /// EntityBase.Id represents PaymentId
        /// </summary>
        public int InterviewRoomId { get; set; }

        public int IntervieweeId { get; set; }

        public int InterviewerId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Pending, Completed, Failed, Refunded
    /// </summary>
    public PaymentStatus Status { get; set; }
    }
}
