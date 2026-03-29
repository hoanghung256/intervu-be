using System;

namespace Intervu.Application.Interfaces.Services
{
    public sealed record AudioProcessingResult(bool Success, byte[] Data, string? Error)
    {
        public static AudioProcessingResult Ok(byte[] data) => new(true, data, null);
        public static AudioProcessingResult Fail(string error) => new(false, Array.Empty<byte>(), error);
    }
}
