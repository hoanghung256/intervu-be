using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Ai
{
    public class LlmTokenUsageDto
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class AiQuestionExtractionResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = string.Empty;

        [JsonPropertyName("question_list")]
        public List<AiQuestionDto> QuestionList { get; set; } = new();

        [JsonPropertyName("usage")]
        public LlmTokenUsageDto? Usage { get; set; }

        [JsonIgnore]
        public string? Error { get; set; }
    }

    public class AiQuestionDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }

    public class NewQuestionsFromTranscriptRequest
    {
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
    }

    public class TranscriptRequest
    {
        public Guid RecordingSessionId { get; set; }
    }

    public class StoreAudioChunkRequest
    {
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
        public Guid RecordingSessionId { get; set; }
        public int SequenceNumber { get; set; } = 0;
    }

    public class UploadAudioFileDebugRequest
    {
        public IFormFile File { get; set; } = null!;
        public Guid RecordingSessionId { get; set; }
    }
}
