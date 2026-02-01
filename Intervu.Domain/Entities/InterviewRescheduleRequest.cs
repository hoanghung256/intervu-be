using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Domain.Entities
{
    public class InterviewRescheduleRequest : EntityBase<Guid>
    {
        public Guid InterviewRoomId { get; set; }

        public Guid CurrentAvailabilityId { get; set; }

        public Guid ProposedAvailabilityId { get; set; }

        public Guid RequestedBy { get; set; }

        public Guid? RespondedBy { get; set; }

        public DateTime? RespondedAt { get; set; }

        public RescheduleRequestStatus Status { get; set; }

        public string? Reason { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime ExpiresAt { get; set; }

        // Navigation Properties
        public InterviewRoom? InterviewRoom { get; set; }

        public CoachAvailability? CurrentAvailability { get; set; }

        public CoachAvailability? ProposedAvailability { get; set; }

        public User? Requester { get; set; }

        public User? Responder { get; set; }
    }
}
