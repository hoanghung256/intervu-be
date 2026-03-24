using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Ai;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.UseCases.AudioChunk;
using Intervu.Application.Interfaces.Services;

namespace Intervu.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IGetQuestionList _getQuestionList;
        private readonly ILogger<AiController> _logger;
        private readonly IStoreAudioChunk _storeAudioChunk;
        private readonly IGetAudioChunk _getAudioChunk;
        private readonly IAudioProcessingService _audioProcessingService;

        public AiController(
            IAiService aiService,
            IGetQuestionList getQuestionList,
            ILogger<AiController> logger,
            IStoreAudioChunk storeAudioChunk,
            IGetAudioChunk getAudioChunk,
            IAudioProcessingService audioProcessingService)
        {
            _aiService = aiService;
            _getQuestionList = getQuestionList;
            _logger = logger;
            _storeAudioChunk = storeAudioChunk;
            _getAudioChunk = getAudioChunk;
            _audioProcessingService = audioProcessingService;
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("transcript/questions")]
        public async Task<IActionResult> GetNewQuestionsFromTranscript([FromBody] TranscriptRequest request)
        {
            // Get all audio chunks for the recording session and merge them
            var audioChunks = await _getAudioChunk.ExecuteAllByRecordingSessionAsync(request.RecordingSessionId);
            
            if (audioChunks == null || audioChunks.Count == 0)
            {
                return NotFound(new { success = false, message = "No audio chunks found for this recording session" });
            }

            var mergeResult = _audioProcessingService.MergeAllTakesAsPcm16kMono(audioChunks);
            if (!mergeResult.Success)
            {
                return Conflict(new { success = false, message = mergeResult.Error });
            }

            _logger.LogInformation("Playing full recording session ID: {SessionId}, Total chunks: {ChunkCount}, Merged size: {Size}", 
                request.RecordingSessionId, audioChunks.Count, mergeResult.Data.Length);
            
            var result = await _aiService.GetNewQuestionsFromTranscriptAsync(mergeResult.Data, request.RecordingSessionId);
            
            if (result.Status == "failed")
            {
                var errorMsg = result.Error ?? "Unknown error from AI service";
                _logger.LogError("AI transcript extraction failed: {Message}", errorMsg);
                return BadRequest(new { success = false, message = errorMsg });
            }
            
            _logger.LogInformation("AI transcript questions extracted successfully");
            
            return Ok(new { success = result.Status, message = "Successfully extract question from transcript"});
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPost("store-audio-chunk")]
        public async Task<IActionResult> StoreAudioChunk([FromBody] StoreAudioChunkRequest request)
        {
            if (request == null || request.AudioData.Length == 0)
            {
                return BadRequest(new { success = false, message = "Audio data is required" });
            }

            if (request.RecordingSessionId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Recording session ID is required" });
            }

            var id = await _storeAudioChunk.ExecuteAsync(request.AudioData, request.RecordingSessionId, request.SequenceNumber);

            _logger.LogInformation("Stored audio chunk with ID: {Id}, SessionId: {SessionId}, Sequence: {Seq}, Length: {Length}", 
                id, request.RecordingSessionId, request.SequenceNumber, request.AudioData.Length);

            return Ok(new { success = true, message = "Audio chunk stored successfully", data = new { id } });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpGet("play-recording/{recordingSessionId}")]
        public async Task<IActionResult> PlayFullRecording(Guid recordingSessionId)
        {
            // Get all audio chunks for the recording session and merge them
            var audioChunks = await _getAudioChunk.ExecuteAllByRecordingSessionAsync(recordingSessionId);
            
            if (audioChunks == null || audioChunks.Count == 0)
            {
                return NotFound(new { success = false, message = "No audio chunks found for this recording session" });
            }

            var mergeResult = _audioProcessingService.MergeAllTakesAsPcm16kMono(audioChunks);
            if (!mergeResult.Success)
            {
                return Conflict(new { success = false, message = mergeResult.Error });
            }

            _logger.LogInformation("Playing full recording session ID: {SessionId}, Total chunks: {ChunkCount}, Merged size: {Size}", 
                recordingSessionId, audioChunks.Count, mergeResult.Data.Length);

            return File(mergeResult.Data, "audio/wav", $"recording-{recordingSessionId}.wav");
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpGet("play-recording-latest/{recordingSessionId}")]
        public async Task<IActionResult> PlayLatestRecordingTake(Guid recordingSessionId)
        {
            var audioChunks = await _getAudioChunk.ExecuteAllByRecordingSessionAsync(recordingSessionId);

            if (audioChunks == null || audioChunks.Count == 0)
            {
                return NotFound(new { success = false, message = "No audio chunks found for this recording session" });
            }

            var mergeResult = _audioProcessingService.MergeLatestTakeAsPcm16kMono(audioChunks);
            if (!mergeResult.Success)
            {
                return Conflict(new { success = false, message = mergeResult.Error });
            }

            _logger.LogInformation("Playing latest take for session ID: {SessionId}, Total chunks: {ChunkCount}, Merged size: {Size}", 
                recordingSessionId, audioChunks.Count, mergeResult.Data.Length);

            return File(mergeResult.Data, "audio/wav", $"recording-latest-{recordingSessionId}.wav");
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpGet("play-audio-chunk/{id}")]
        public async Task<IActionResult> PlayAudioChunk(Guid id)
        {
            var audioChunk = await _getAudioChunk.ExecuteAsync(id);
            if (audioChunk == null)
            {
                return NotFound(new { success = false, message = "Audio chunk not found" });
            }

            // Assuming the audio is in WAV format; adjust content type if different
            return File(audioChunk.AudioData, "audio/wav", $"audio-{id}.wav");
        }
    }
}
