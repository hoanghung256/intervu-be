using Intervu.Application.DTOs.Ai;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.CandidateProfile
{
    public class EvaluateCandidateCv : IEvaluateCandidateCv
    {
        private readonly IAiService _aiService;
        private readonly ICandidateProfileRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EvaluateCandidateCv> _logger;

        public EvaluateCandidateCv(
            IAiService aiService, 
            ICandidateProfileRepository repository,
            IHttpClientFactory httpClientFactory,
            ILogger<EvaluateCandidateCv> logger)
        {
            _aiService = aiService;
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<AiCvEvaluationResponseDto?> ExecuteAsync(Guid candidateId, IFormFile? file = null)
        {
            var profile = await _repository.GetByIdAsync(candidateId);
            if (profile == null)
            {
                _logger.LogWarning("Candidate profile not found for ID: {CandidateId}", candidateId);
                return null;
            }

            // 1. One-off Evaluation with Uploaded File (No Save)
            if (file != null && file.Length > 0)
            {
                _logger.LogInformation("Starting one-off CV evaluation for candidate: {CandidateId} with file: {FileName}", candidateId, file.FileName);
                try
                {
                    using var stream = file.OpenReadStream();
                    var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/pdf" : file.ContentType;
                    return await _aiService.EvaluateCvAsync(stream, file.FileName, contentType, useCase: "CvEvaluation");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error evaluating uploaded file for candidate: {CandidateId}", candidateId);
                    return new AiCvEvaluationResponseDto { Error = $"Failed to evaluate uploaded file: {ex.Message}" };
                }
            }

            // 2. Current CV Evaluation (from DB/URL)
            // Load from DB Cache if available (unless we want to force re-evaluation, but user requested "it will load from db")
            if (!string.IsNullOrEmpty(profile.AIEvaluation))
            {
                _logger.LogInformation("Found cached AI evaluation for candidate: {CandidateId}. Returning from DB. \n EvaluationResults: {AIEvaluation}", candidateId, profile.AIEvaluation);
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<AiCvEvaluationResponseDto>(profile.AIEvaluation, options);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached AI evaluation for candidate: {CandidateId}. Re-evaluating.", candidateId);
                    // If JSON is corrupted, proceed to re-evaluate
                }
            }

            if (string.IsNullOrEmpty(profile.CVUrl))
            {
                _logger.LogWarning("CV URL is missing for candidate: {CandidateId}. Cannot evaluate.", candidateId);
                return new AiCvEvaluationResponseDto 
                { 
                    Error = "Please upload a cv before using this feature" 
                };
            }

            _logger.LogInformation("No cache found. Fetching CV from URL for evaluation. Candidate: {CandidateId}, URL: {CVUrl}", candidateId, profile.CVUrl);
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                using var stream = await httpClient.GetStreamAsync(profile.CVUrl);

                var evaluationResult = await _aiService.EvaluateCvAsync(stream, "cv.pdf", "application/pdf", useCase: "CvEvaluation");

                if (evaluationResult == null || !string.IsNullOrEmpty(evaluationResult.Error))
                {
                    _logger.LogWarning("AI evaluation returned error or null for candidate: {CandidateId}. Error: {Error}", candidateId, evaluationResult?.Error);
                    return evaluationResult;
                }

                _logger.LogInformation("Successfully evaluated CV for candidate: {CandidateId}. Saving to DB cache. \n EvaluationResults: {AIEvaluation}", candidateId, evaluationResult);
                // Store in Candidate Profile for caching
                profile.AIEvaluation = JsonSerializer.Serialize(evaluationResult);
                await _repository.UpdateCandidateProfileAsync(profile);

                return evaluationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure during CV retrieval or evaluation for candidate: {CandidateId}", candidateId);
                return new AiCvEvaluationResponseDto { Error = $"Failed to load or evaluate CV: {ex.Message}" };
            }
        }
    }
}
