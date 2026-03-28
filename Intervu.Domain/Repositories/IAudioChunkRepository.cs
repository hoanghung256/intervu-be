using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface IAudioChunkRepository : IRepositoryBase<AudioChunk>
    {
        // Add specific methods if needed
        Task<List<AudioChunk>> GetByRecordingSessionIdAsync(Guid recordingSessionId);
    }
}