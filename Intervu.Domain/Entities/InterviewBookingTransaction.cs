using Intervu.Domain.Abstractions.Entities;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities
{
    public class InterviewBookingTransaction : EntityBase<int>
    {
        public int UserId { get; set; }

        public int InterviewerAvailabilityId { get; set; }

        public int Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
