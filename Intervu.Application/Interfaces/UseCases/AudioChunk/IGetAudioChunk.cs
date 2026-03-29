using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.AudioChunk
{
    public interface IGetAudioChunk
    {
        Task<Intervu.Domain.Entities.AudioChunk?> ExecuteAsync(Guid id);
        Task<List<Intervu.Domain.Entities.AudioChunk>> ExecuteAllByRecordingSessionAsync(Guid recordingSessionId);
    }
}