using System;
using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Domain.Entities
{
    public class AuditLog : EntityBase<Guid>
    {
        public Guid? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MetaData { get; set; } // JSON string for additional information
        public AuditLogEventType EventType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
