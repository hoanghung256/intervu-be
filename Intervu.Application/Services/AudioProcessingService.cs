using Intervu.Application.Interfaces.Services;
using Intervu.Domain.Entities;
using NAudio.Wave;
using NAudio.Lame;
using Concentus.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Intervu.Application.Services
{
    public class AudioProcessingService : IAudioProcessingService
    {
        private const int TargetSampleRate = 48000; // Opus default
        private const int TargetChannels = 1;
        private const int TargetMp3Bitrate = 128;

        public AudioProcessingResult MergeLatestTakeAsMp3(List<AudioChunk> chunks)
        {
            var latestTake = SelectLatestTake(chunks);
            return MergeTakesAsMp3(new List<List<AudioChunk>> { latestTake });
        }

        public AudioProcessingResult MergeAllTakesAsMp3(List<AudioChunk> chunks)
        {
            var takes = SplitIntoTakes(chunks);
            return MergeTakesAsMp3(takes);
        }

        public AudioProcessingResult MergeLatestTakeAsWav(List<AudioChunk> chunks)
        {
            var latestTake = SelectLatestTake(chunks);
            return MergeTakesAsWav(new List<List<AudioChunk>> { latestTake });
        }

        public AudioProcessingResult MergeAllTakesAsWav(List<AudioChunk> chunks)
        {
            var takes = SplitIntoTakes(chunks);
            return MergeTakesAsWav(takes);
        }

        private AudioProcessingResult MergeTakesAsMp3(List<List<AudioChunk>> takes)
        {
            if (takes.Count == 0) return AudioProcessingResult.Ok(Array.Empty<byte>());

            try
            {
                byte[] pcmData = DecodeWebmTakesToPcm(takes);
                if (pcmData.Length == 0) return AudioProcessingResult.Ok(Array.Empty<byte>());

                using var mp3Stream = new MemoryStream();
                var waveFormat = new WaveFormat(TargetSampleRate, 16, TargetChannels);
                using (var writer = new LameMP3FileWriter(mp3Stream, waveFormat, TargetMp3Bitrate))
                {
                    writer.Write(pcmData, 0, pcmData.Length);
                }
                return AudioProcessingResult.Ok(mp3Stream.ToArray());
            }
            catch (Exception ex)
            {
                return AudioProcessingResult.Fail($"MP3 Transcoding failed: {ex.Message}");
            }
        }

        private AudioProcessingResult MergeTakesAsWav(List<List<AudioChunk>> takes)
        {
            if (takes.Count == 0) return AudioProcessingResult.Ok(Array.Empty<byte>());

            try
            {
                byte[] pcmData = DecodeWebmTakesToPcm(takes);
                if (pcmData.Length == 0) return AudioProcessingResult.Ok(Array.Empty<byte>());

                using var wavStream = new MemoryStream();
                var waveFormat = new WaveFormat(TargetSampleRate, 16, TargetChannels);
                using (var writer = new WaveFileWriter(wavStream, waveFormat))
                {
                    writer.Write(pcmData, 0, pcmData.Length);
                }
                return AudioProcessingResult.Ok(wavStream.ToArray());
            }
            catch (Exception ex)
            {
                return AudioProcessingResult.Fail($"WAV Transcoding failed: {ex.Message}");
            }
        }

        private byte[] DecodeWebmTakesToPcm(List<List<AudioChunk>> takes)
        {
            using var pcmCollector = new MemoryStream();
            var decoder = new OpusDecoder(TargetSampleRate, TargetChannels);

            // Opus max frame size is 120ms at 48kHz = 5760 samples
            short[] outBuffer = new short[5760];
            byte[] outByteBuf = new byte[outBuffer.Length * 2];

            foreach (var take in takes)
            {
                var fullTakeBlob = new MemoryStream();
                foreach (var chunk in take.OrderBy(c => c.ChunkSequenceNumber))
                {
                    fullTakeBlob.Write(chunk.AudioData, 0, chunk.AudioData.Length);
                }
                fullTakeBlob.Position = 0;

                // Simple EBML/WebM Demuxer to find SimpleBlocks (0xA3)
                using var reader = new BinaryReader(fullTakeBlob);
                while (fullTakeBlob.Position < fullTakeBlob.Length)
                {
                    try
                    {
                        long id = ReadEbmlId(reader);
                        long size = ReadEbmlVInt(reader);

                        if (id == 0xA3) // SimpleBlock
                        {
                            long blockStart = fullTakeBlob.Position;
                            ReadEbmlVInt(reader); // Track Number (skip)
                            reader.ReadInt16(); // Timecode (skip)
                            reader.ReadByte(); // Flags (skip)

                            int headerSize = (int)(fullTakeBlob.Position - blockStart);
                            int opusSize = (int)size - headerSize;

                            if (opusSize > 0)
                            {
                                byte[] opusPacket = reader.ReadBytes(opusSize);
                                int decodedSamples = decoder.Decode(opusPacket, 0, opusPacket.Length, outBuffer, 0, outBuffer.Length, false);

                                for (int i = 0; i < decodedSamples; i++)
                                {
                                    BitConverter.TryWriteBytes(outByteBuf.AsSpan(i * 2), outBuffer[i]);
                                }
                                pcmCollector.Write(outByteBuf, 0, decodedSamples * 2);
                            }
                        }
                        else if (id == 0x1A45DFA3 || id == 0x18538067 || id == 0x1F43B675 || id == 0x1654AE6B || id == 0x1549A966)
                        {
                            // Container tags (Header, Segment, Cluster, Tracks, Info) - dive in
                            continue;
                        }
                        else
                        {
                            // Skip other elements
                            fullTakeBlob.Position += size;
                        }
                    }
                    catch { break; }
                }
            }

            return pcmCollector.ToArray();
        }

        private static long ReadEbmlId(BinaryReader reader)
        {
            byte b = reader.ReadByte();
            if ((b & 0x80) != 0) return b;
            if ((b & 0x40) != 0) return (long)b << 8 | reader.ReadByte();
            if ((b & 0x20) != 0) return (long)b << 16 | reader.ReadByte() << 8 | reader.ReadByte();
            return (long)b << 24 | reader.ReadByte() << 16 | reader.ReadByte() << 8 | reader.ReadByte();
        }

        private static long ReadEbmlVInt(BinaryReader reader)
        {
            byte b = reader.ReadByte();
            int length = 0;
            if ((b & 0x80) != 0) length = 1;
            else if ((b & 0x40) != 0) length = 2;
            else if ((b & 0x20) != 0) length = 3;
            else if ((b & 0x10) != 0) length = 4;
            else if ((b & 0x08) != 0) length = 5;
            else if ((b & 0x04) != 0) length = 6;
            else if ((b & 0x02) != 0) length = 7;
            else if ((b & 0x01) != 0) length = 8;

            long val = b & (0xFF >> length);
            for (int i = 1; i < length; i++)
            {
                val = (val << 8) | reader.ReadByte();
            }
            return val;
        }

        private static List<List<AudioChunk>> SplitIntoTakes(List<AudioChunk> chunks)
        {
            var result = new List<List<AudioChunk>>();
            if (chunks == null || chunks.Count == 0) return result;
            var ordered = chunks.OrderBy(c => c.CreatedAt).ThenBy(c => c.ChunkSequenceNumber).ToList();
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
            if (current.Count > 0) result.Add(current);
            return result;
        }

        private static List<AudioChunk> SelectLatestTake(List<AudioChunk> chunks)
        {
            if (chunks == null || chunks.Count == 0) return new List<AudioChunk>();
            var ordered = chunks.OrderBy(c => c.CreatedAt).ThenBy(c => c.ChunkSequenceNumber).ToList();
            var lastStartIndex = ordered.FindLastIndex(c => c.ChunkSequenceNumber == 0);
            return (lastStartIndex < 0) ? ordered : ordered.Skip(lastStartIndex).ToList();
        }
    }
}
