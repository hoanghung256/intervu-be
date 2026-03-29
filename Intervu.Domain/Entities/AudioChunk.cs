using Intervu.Domain.Abstractions.Entity;
using System;

namespace Intervu.Domain.Entities
{
    public class AudioChunk : EntityBase<Guid>
    {
        public required byte[] AudioData { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Track which recording/session this chunk belongs to
        public Guid RecordingSessionId { get; set; }
        
        public int ChunkSequenceNumber { get; set; } = 0;
    }
}