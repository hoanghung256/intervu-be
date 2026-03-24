using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Entities;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
        private const int TargetBitsPerSample = 16;

        public AudioProcessingResult MergeLatestTakeAsPcm16kMono(List<AudioChunk> chunks)
        {
            var latestTake = SelectLatestTake(chunks);
            return MergeTakeAsPcm16kMono(latestTake);
        }

        public AudioProcessingResult MergeAllTakesAsPcm16kMono(List<AudioChunk> chunks)
        {
            var takes = SplitIntoTakes(chunks);
            if (takes.Count == 0)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            var convertedTakes = new List<byte[]>();
            foreach (var take in takes)
            {
                var takeResult = MergeTakeAsPcm16kMono(take);
                if (!takeResult.Success)
                    return takeResult;

                if (takeResult.Data.Length > 0)
                    convertedTakes.Add(takeResult.Data);
            }

            return MergeWavFiles(convertedTakes);
        }

        private AudioProcessingResult MergeTakeAsPcm16kMono(List<AudioChunk> take)
        {
            if (take == null || take.Count == 0)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            try
            {
                var mergedWav = MergeWavChunks(take.Select(c => c.AudioData).ToList());
                var converted = ConvertToPcm16kMono(mergedWav);
                return AudioProcessingResult.Ok(converted);
            }
            catch (Exception ex)
            {
                return AudioProcessingResult.Fail($"Audio conversion failed: {ex.Message}");
            }
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
            if (lastStartIndex <= 0)
                return ordered;

            return ordered
                .Skip(lastStartIndex)
                .OrderBy(c => c.ChunkSequenceNumber)
                .ThenBy(c => c.CreatedAt)
                .ToList();
        }

        private static AudioProcessingResult MergeWavFiles(List<byte[]> wavFiles)
        {
            if (wavFiles == null || wavFiles.Count == 0)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            var mergedData = new List<byte>();
            byte[]? header = null;
            int headerDataOffset = 0;
            WavFormat? format = null;

            foreach (var wav in wavFiles)
            {
                if (wav == null || wav.Length == 0)
                    continue;

                if (!TryGetWavDataOffset(wav, out var dataOffset) || !TryGetWavFormat(wav, out var wavFormat))
                {
                    return AudioProcessingResult.Fail("One or more takes are not valid WAV files");
                }

                if (format == null)
                {
                    format = wavFormat;
                    headerDataOffset = dataOffset;
                    header = wav.Take(dataOffset).ToArray();
                }
                else if (!format.Value.Equals(wavFormat))
                {
                    return AudioProcessingResult.Fail("Takes have different WAV formats (sample rate, channels, or bit depth)");
                }

                if (wav.Length > dataOffset)
                {
                    mergedData.AddRange(wav.Skip(dataOffset));
                }
            }

            if (header == null)
                return AudioProcessingResult.Ok(Array.Empty<byte>());

            var totalDataSize = mergedData.Count;
            UpdateWavHeaderSizes(header, headerDataOffset, totalDataSize);

            var merged = new byte[header.Length + totalDataSize];
            Buffer.BlockCopy(header, 0, merged, 0, header.Length);
            mergedData.CopyTo(merged, header.Length);
            return AudioProcessingResult.Ok(merged);
        }

        private static byte[] ConvertToPcm16kMono(byte[] wavBytes)
        {
            using var input = new MemoryStream(wavBytes, writable: false);
            using var reader = new WaveFileReader(input);

            ISampleProvider sampleProvider = reader.ToSampleProvider();

            if (sampleProvider.WaveFormat.SampleRate != TargetSampleRate)
            {
                sampleProvider = new WdlResamplingSampleProvider(sampleProvider, TargetSampleRate);
            }

            if (sampleProvider.WaveFormat.Channels == 2)
            {
                var stereoToMono = new StereoToMonoSampleProvider(sampleProvider)
                {
                    LeftVolume = 0.5f,
                    RightVolume = 0.5f
                };
                sampleProvider = stereoToMono;
            }
            else if (sampleProvider.WaveFormat.Channels != TargetChannels)
            {
                throw new InvalidOperationException("Only mono or stereo WAV formats are supported");
            }

            var pcmProvider = new SampleToWaveProvider16(sampleProvider);
            using var output = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(output, pcmProvider);
            return output.ToArray();
        }

        private static byte[] MergeWavChunks(List<byte[]> chunks)
        {
            if (chunks == null || chunks.Count == 0)
                return Array.Empty<byte>();

            if (chunks.Count == 1)
                return chunks[0];

            var mergedData = new List<byte>();
            byte[]? header = null;
            int headerDataOffset = 0;

            foreach (var chunk in chunks)
            {
                if (chunk == null || chunk.Length == 0)
                    continue;

                if (!TryGetWavDataOffset(chunk, out var dataOffset))
                    throw new InvalidOperationException("Invalid WAV chunk");

                if (header == null)
                {
                    headerDataOffset = dataOffset;
                    header = chunk.Take(dataOffset).ToArray();
                }

                if (chunk.Length > dataOffset)
                {
                    mergedData.AddRange(chunk.Skip(dataOffset));
                }
            }

            if (header == null)
                return Array.Empty<byte>();

            var totalDataSize = mergedData.Count;
            UpdateWavHeaderSizes(header, headerDataOffset, totalDataSize);

            var result = new byte[header.Length + totalDataSize];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            mergedData.CopyTo(result, header.Length);
            return result;
        }

        private static bool TryGetWavDataOffset(byte[] wavBytes, out int dataOffset)
        {
            dataOffset = 0;
            if (wavBytes.Length < 12)
                return false;

            if (wavBytes[0] != (byte)'R' || wavBytes[1] != (byte)'I' || wavBytes[2] != (byte)'F' || wavBytes[3] != (byte)'F')
                return false;

            if (wavBytes[8] != (byte)'W' || wavBytes[9] != (byte)'A' || wavBytes[10] != (byte)'V' || wavBytes[11] != (byte)'E')
                return false;

            var offset = 12;
            while (offset + 8 <= wavBytes.Length)
            {
                var id0 = wavBytes[offset];
                var id1 = wavBytes[offset + 1];
                var id2 = wavBytes[offset + 2];
                var id3 = wavBytes[offset + 3];

                var chunkSize = BitConverter.ToInt32(wavBytes, offset + 4);
                if (chunkSize < 0)
                    return false;

                var next = offset + 8 + chunkSize;
                if (next > wavBytes.Length)
                    return false;

                if (id0 == (byte)'d' && id1 == (byte)'a' && id2 == (byte)'t' && id3 == (byte)'a')
                {
                    dataOffset = offset + 8;
                    return true;
                }

                offset = next;
            }

            return false;
        }

        private static bool TryGetWavFormat(byte[] wavBytes, out WavFormat format)
        {
            format = default;
            if (wavBytes.Length < 12)
                return false;

            var offset = 12;
            while (offset + 8 <= wavBytes.Length)
            {
                var id0 = wavBytes[offset];
                var id1 = wavBytes[offset + 1];
                var id2 = wavBytes[offset + 2];
                var id3 = wavBytes[offset + 3];

                var chunkSize = BitConverter.ToInt32(wavBytes, offset + 4);
                if (chunkSize < 0)
                    return false;

                var next = offset + 8 + chunkSize;
                if (next > wavBytes.Length)
                    return false;

                if (id0 == (byte)'f' && id1 == (byte)'m' && id2 == (byte)'t' && id3 == (byte)' ')
                {
                    if (chunkSize < 16 || offset + 8 + 16 > wavBytes.Length)
                        return false;

                    var fmtOffset = offset + 8;
                    format = new WavFormat(
                        BitConverter.ToUInt16(wavBytes, fmtOffset),
                        BitConverter.ToUInt16(wavBytes, fmtOffset + 2),
                        BitConverter.ToInt32(wavBytes, fmtOffset + 4),
                        BitConverter.ToInt32(wavBytes, fmtOffset + 8),
                        BitConverter.ToUInt16(wavBytes, fmtOffset + 12),
                        BitConverter.ToUInt16(wavBytes, fmtOffset + 14)
                    );
                    return true;
                }

                offset = next;
            }

            return false;
        }

        private static void UpdateWavHeaderSizes(byte[] header, int dataOffset, int totalDataSize)
        {
            var riffSize = header.Length + totalDataSize - 8;
            WriteInt32LittleEndian(header, 4, riffSize);

            var dataSizeOffset = dataOffset - 4;
            if (dataSizeOffset >= 0 && dataSizeOffset + 4 <= header.Length)
            {
                WriteInt32LittleEndian(header, dataSizeOffset, totalDataSize);
            }
        }

        private static void WriteInt32LittleEndian(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private readonly record struct WavFormat(
            ushort AudioFormat,
            ushort NumChannels,
            int SampleRate,
            int ByteRate,
            ushort BlockAlign,
            ushort BitsPerSample
        );
    }
}
