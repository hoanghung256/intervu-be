using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;
using System;

namespace Intervu.Domain.Entities
{
    public class Notification : EntityBase<Guid>
    {
        public Guid UserId { get; set; }

        public NotificationType Type { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        // FE redirect path on click (e.g., /interview?tab=upcoming)
        public string? ActionUrl { get; set; }

        // Source entity ID (RoomId, FeedbackId, etc.) for deep-linking
        public Guid? ReferenceId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
