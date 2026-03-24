using Intervu.Application.Interfaces.UseCases.AudioChunk;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Intervu.Application.UseCases.AudioChunk
{
    public class GetAudioChunk(IAudioChunkRepository audioChunkRepository) : IGetAudioChunk
    {
        public async Task<Intervu.Domain.Entities.AudioChunk?> ExecuteAsync(Guid id)
        {
            return await audioChunkRepository.GetByIdAsync(id);
        }

        public async Task<List<Intervu.Domain.Entities.AudioChunk>> ExecuteAllByRecordingSessionAsync(Guid recordingSessionId)
        {
            // Get all audio chunks for a specific recording session, ordered by sequence and creation time
            return await audioChunkRepository.GetByRecordingSessionIdAsync(recordingSessionId);
        }
    }
}