using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Ai;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.Services;
using Intervu.Application.Interfaces.UseCases.AudioChunk;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.Industry;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Skill;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ILogger<AiController> _logger;
        private readonly IStoreAudioChunk _storeAudioChunk;
        private readonly IGetAudioChunk _getAudioChunk;
        private readonly IAudioProcessingService _audioProcessingService;
        private readonly IStoreGeneratedQuestions _storeGeneratedQuestions;
        private readonly IGetCurrentRoom _getCurrentRoom;
        private readonly ITagRepository _tagRepository;

        public AiController(
            IAiService aiService,
            ILogger<AiController> logger,
            IStoreAudioChunk storeAudioChunk,
            IGetAudioChunk getAudioChunk,
            IAudioProcessingService audioProcessingService,
            IStoreGeneratedQuestions storeGeneratedQuestions,
            IGetCurrentRoom getCurrentRoom,
            ITagRepository tagRepository
            )
        {
            _aiService = aiService;
            _logger = logger;
            _storeAudioChunk = storeAudioChunk;
            _getAudioChunk = getAudioChunk;
            _audioProcessingService = audioProcessingService;
            _storeGeneratedQuestions = storeGeneratedQuestions;
            _getCurrentRoom = getCurrentRoom;
            _tagRepository = tagRepository;
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("transcript/questions")]
        public async Task<IActionResult> GetNewQuestionsFromTranscript([FromBody] TranscriptRequest request)
        {
            var audioChunks = await _getAudioChunk.ExecuteAllByRecordingSessionAsync(request.RecordingSessionId);
            
            if (audioChunks == null || audioChunks.Count == 0)
            {
                return NotFound(new { success = false, message = "No audio chunks found for this recording session" });
            }

            var mergeResult = _audioProcessingService.MergeAllTakesAsWav(audioChunks);
            if (!mergeResult.Success)
            {
                return Conflict(new { success = false, message = mergeResult.Error });
            }

            _logger.LogInformation("Processing transcript for session ID: {SessionId}, Total chunks: {ChunkCount}, Merged size: {Size}", 
                request.RecordingSessionId, audioChunks.Count, mergeResult.Data.Length);
            var dbTags = await _tagRepository.GetAllAsync();
            var availableTags = dbTags.Select(t => t.Name).Distinct().ToList();

            var result = await _aiService.GetNewQuestionsFromTranscriptAsync(mergeResult.Data, request.RecordingSessionId, availableTags, useCase: "InterviewTranscript");
            
            if (result.Status == "failed")
            {
                var errorMsg = result.Error ?? "Unknown error from AI service";
                _logger.LogError("AI transcript extraction failed: {Message}", errorMsg);
                return BadRequest(new { success = false, message = errorMsg });
            }
            
            var storedCount = await _storeGeneratedQuestions.ExecuteAsync(request.RecordingSessionId, result.QuestionList, result.Transcript);

            return Ok(new
            {
                success = result.Status,
                message = "Successfully extract question from transcript",
                data = new { storedCount }
            });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
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

            var room = await _getCurrentRoom.ExecuteAsync(request.RecordingSessionId);

            if (room.Status != InterviewRoomStatus.Ongoing)
            {
                return BadRequest(new { success = false, message = "Room is not active" });
            }

            _logger.LogInformation("Storing audio chunk for session ID: {SessionId}, Sequence: {Sequence}, Size: {Size}",
                request.RecordingSessionId, request.SequenceNumber, request.AudioData.Length);

            var id = await _storeAudioChunk.ExecuteAsync(request.AudioData, request.RecordingSessionId, request.SequenceNumber);

            return Ok(new { success = true, message = "Audio chunk stored successfully", data = new { id } });
        }

#if DEBUG
        /// <summary>
        /// DEBUG ONLY: Upload a full audio file to simulate chunks
        /// </summary>
        [HttpPost("debug/upload-audio-file")]
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAudioFileDebug([FromForm] UploadAudioFileDebugRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            using var ms = new MemoryStream();
            await request.File.CopyToAsync(ms);
            var audioData = ms.ToArray();

            var id = await _storeAudioChunk.ExecuteAsync(audioData, request.RecordingSessionId, 0);

            _logger.LogInformation("DEBUG: Uploaded audio file {FileName} as chunk for session {SessionId}", request.File.FileName, request.RecordingSessionId);

            return Ok(new { success = true, message = "File uploaded and stored as chunk", data = new { id, size = audioData.Length } });
        }
#endif

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpGet("play-recording/{recordingSessionId}")]
        public async Task<IActionResult> PlayFullRecording(Guid recordingSessionId)
        {
            var audioChunks = await _getAudioChunk.ExecuteAllByRecordingSessionAsync(recordingSessionId);
            
            if (audioChunks == null || audioChunks.Count == 0)
            {
                return NotFound(new { success = false, message = "No audio chunks found for this recording session" });
            }

            var mergeResult = _audioProcessingService.MergeAllTakesAsWav(audioChunks);
            if (!mergeResult.Success)
            {
                return Conflict(new { success = false, message = mergeResult.Error });
            }

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

            var mergeResult = _audioProcessingService.MergeLatestTakeAsWav(audioChunks);
            if (!mergeResult.Success)
            {
                return Conflict(new { success = false, message = mergeResult.Error });
            }

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

            return File(audioChunk.AudioData, "audio/webm", $"audio-{id}.webm");
        }
    }
}
