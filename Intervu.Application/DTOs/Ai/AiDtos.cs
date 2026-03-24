using System.Text.Json.Serialization;

namespace Intervu.Application.DTOs.Ai
{
    public class AiQuestionExtractionResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = string.Empty;

        [JsonPropertyName("question list")]
        public List<AiQuestionDto> QuestionList { get; set; } = new();

        [JsonIgnore]
        public string? Error { get; set; }
    }

    public class AiQuestionDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
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
}
