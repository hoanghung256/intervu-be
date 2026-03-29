using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Entities;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Lame; // Requires NAudio.Lame NuGet package
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Intervu.Application.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private const int TargetSampleRate = 16000;
        private const int TargetChannels = 1;
        private const int TargetMp3Bitrate = 128; // Bitrate for speech

        public AudioProcessingResult MergeLatestTakeAsMp3(List<AudioChunk> chunks)
        {
            var latestTake = SelectLatestTake(chunks);
            return MergeTakeAsMp3(latestTake);
        }

        public AudioProcessingResult MergeAllTakesAsMp3(List<AudioChunk> chunks)
        {
            var takes = SplitIntoTakes(chunks);
            if (takes.Count == 0)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            try
            {
                using var outputStream = new MemoryStream();
                // Create a standard format for intermediate PCM data
                var waveFormat = new WaveFormat(TargetSampleRate, 16, TargetChannels);
                
                using (var pcmCollector = new MemoryStream())
                {
                    foreach (var take in takes)
                    {
                        foreach (var chunk in take.OrderBy(c => c.ChunkSequenceNumber))
                        {
                            var pcmData = ConvertToRawPcm16kMono(chunk.AudioData);
                            pcmCollector.Write(pcmData, 0, pcmData.Length);
                        }
                    }

                    // Now encode the collected PCM to MP3
                    return EncodePcmToMp3(pcmCollector.ToArray(), waveFormat);
                }
            }
            catch (Exception ex)
            {
                return AudioProcessingResult.Fail($"Audio processing failed: {ex.Message}");
            }
        }

        private AudioProcessingResult MergeTakeAsMp3(List<AudioChunk> take)
        {
            if (take == null || take.Count == 0)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            try
            {
                using var pcmCollector = new MemoryStream();
                foreach (var chunk in take.OrderBy(c => c.ChunkSequenceNumber))
                {
                    var pcmData = ConvertToRawPcm16kMono(chunk.AudioData);
                    pcmCollector.Write(pcmData, 0, pcmData.Length);
                }

                return EncodePcmToMp3(pcmCollector.ToArray(), new WaveFormat(TargetSampleRate, 16, TargetChannels));
            }
            catch (Exception ex)
            {
                return AudioProcessingResult.Fail($"Audio conversion to MP3 failed: {ex.Message}");
            }
        }

        private byte[] ConvertToRawPcm16kMono(byte[] audioData)
        {
            using var ms = new MemoryStream(audioData);
            WaveStream reader;

            if (IsWav(audioData))
                reader = new WaveFileReader(ms);
            else
                reader = new Mp3FileReader(ms);

            using (reader)
            {
                ISampleProvider sampleProvider = reader.ToSampleProvider();

                if (sampleProvider.WaveFormat.SampleRate != TargetSampleRate)
                    sampleProvider = new WdlResamplingSampleProvider(sampleProvider, TargetSampleRate);

                if (sampleProvider.WaveFormat.Channels > TargetChannels)
                    sampleProvider = new StereoToMonoSampleProvider(sampleProvider);

                var pcm16Provider = new SampleToWaveProvider16(sampleProvider);
                
                using var pcmStream = new MemoryStream();
                byte[] buffer = new byte[TargetSampleRate * 2]; // 1 second buffer
                int bytesRead;
                while ((bytesRead = pcm16Provider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    pcmStream.Write(buffer, 0, bytesRead);
                }
                return pcmStream.ToArray();
            }
        }

        private AudioProcessingResult EncodePcmToMp3(byte[] pcmData, WaveFormat pcmFormat)
        {
            if (pcmData == null || pcmData.Length == 0)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            try
            {
                using var output = new MemoryStream();
                using (var writer = new LameMP3FileWriter(output, pcmFormat, TargetMp3Bitrate))
                {
                    writer.Write(pcmData, 0, pcmData.Length);
                }
                return AudioProcessingResult.Ok(output.ToArray());
            }
            catch (Exception ex)
            {
                return AudioProcessingResult.Fail($"MP3 encoding failed: {ex.Message}");
            }
        }

        private static bool IsWav(byte[] data)
        {
            return data.Length > 12 && 
                   data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F' &&
                   data[8] == (byte)'W' && data[9] == (byte)'A' && data[10] == (byte)'V' && data[11] == (byte)'E';
        }

        private static List<List<AudioChunk>> SplitIntoTakes(List<AudioChunk> chunks)
        {
            var result = new List<List<AudioChunk>>();
            if (chunks == null || chunks.Count == 0)
                return result;

            var ordered = chunks
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.ChunkSequenceNumber)
                .ToList();

            var current = new List<AudioChunk>();
            foreach (var chunk in ordered)
            {
                if (current.Count > 0 && chunk.ChunkSequenceNumber == 0)
                {
                    result.Add(current);
                    current = new List<AudioChunk>();
                }

                current.Add(chunk);
            }

            if (current.Count > 0)
                result.Add(current);

            return result;
        }

        private static List<AudioChunk> SelectLatestTake(List<AudioChunk> chunks)
        {
            if (chunks == null || chunks.Count == 0)
                return new List<AudioChunk>();

            var ordered = chunks
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.ChunkSequenceNumber)
                .ToList();

            var lastStartIndex = ordered.FindLastIndex(c => c.ChunkSequenceNumber == 0);
            if (lastStartIndex < 0)
                return ordered;

            return ordered
                .Skip(lastStartIndex)
                .OrderBy(c => c.ChunkSequenceNumber)
                .ThenBy(c => c.CreatedAt)
                .ToList();
        }
    }
}
