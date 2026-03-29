using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class AudioChunkRepository : RepositoryBase<AudioChunk>, IAudioChunkRepository
    {
        public AudioChunkRepository(IntervuPostgreDbContext context) : base(context)
        {
        }

        public async Task<List<AudioChunk>> GetByRecordingSessionIdAsync(Guid recordingSessionId)
        {
            return await _context.AudioChunks
                .Where(c => c.RecordingSessionId == recordingSessionId)
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.ChunkSequenceNumber)
                .ToListAsync();
        }
    }
}
