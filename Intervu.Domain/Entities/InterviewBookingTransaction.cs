using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities
{
    public class InterviewBookingTransaction : EntityBase<Guid>
    {
        // For tracking with PayOS, IDENTITY(1,1)
        public int OrderCode { get; set; }

        public Guid UserId { get; set; }

        public Guid CoachAvailabilityId { get; set; }

        public int Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }
    }
}
