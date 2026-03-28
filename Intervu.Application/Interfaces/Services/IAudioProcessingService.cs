using Intervu.Domain.Entities;
using System.Collections.Generic;

namespace Intervu.Application.Interfaces.Services
{
    public interface IAudioProcessingService
    {
        AudioProcessingResult MergeLatestTakeAsMp3(List<AudioChunk> chunks);
        AudioProcessingResult MergeAllTakesAsMp3(List<AudioChunk> chunks);
    }
}
