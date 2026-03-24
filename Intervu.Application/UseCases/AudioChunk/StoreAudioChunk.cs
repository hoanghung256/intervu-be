using Intervu.Application.Interfaces.UseCases.AudioChunk;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.AudioChunk
{
    public class StoreAudioChunk(IAudioChunkRepository audioChunkRepository) : IStoreAudioChunk
    {
        public async Task<Guid> ExecuteAsync(byte[] audioData, Guid recordingSessionId, int sequenceNumber = 0)
        {
            var audioChunk = new Intervu.Domain.Entities.AudioChunk
            {
                Id = Guid.NewGuid(),
                AudioData = audioData,
                CreatedAt = DateTime.UtcNow,
                RecordingSessionId = recordingSessionId,
                ChunkSequenceNumber = sequenceNumber
            };

            await audioChunkRepository.AddAsync(audioChunk);
            await audioChunkRepository.SaveChangesAsync();

            return audioChunk.Id;
        }
    }
}