using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.AudioChunk
{
    public interface IStoreAudioChunk
    {
        Task<Guid> ExecuteAsync(byte[] audioData, Guid recordingSessionId, int sequenceNumber = 0);
    }
}