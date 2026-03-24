using Intervu.Domain.Entities;
using System.Collections.Generic;

namespace Intervu.Application.Interfaces.Services
{
    public interface IAudioProcessingService
    {
        AudioProcessingResult MergeLatestTakeAsPcm16kMono(List<AudioChunk> chunks);
        AudioProcessingResult MergeAllTakesAsPcm16kMono(List<AudioChunk> chunks);
    }
}
