using System.Threading;
using System.Threading.Tasks;
using Intervu.Application.DTOs;
using Intervu.Application.DTOs.Assessment;
using System;
using Intervu.Application.DTOs.Ai;
using Intervu.Application.DTOs.Question;
using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IAiService
    {
        Task<GenerateAssessmentResponse> GenerateAssessmentAsync(GenerateAssessmentRequest request, string? useCase = null);
        Task<string> EvaluateAssessmentRawAsync(EvaluateAssessmentRequestDto request, CancellationToken cancellationToken = default, string? useCase = null);
        Task<AiGenerateRoadmapResponseDto?> GenerateRoadmapAsync(AiGenerateRoadmapRequestDto request, CancellationToken cancellationToken = default, string? useCase = null);
        Task<AiUpdateRoadmapProgressResponseDto?> UpdateRoadmapProgressAsync(AiUpdateRoadmapProgressRequestDto request, CancellationToken cancellationToken = default, string? useCase = null);
        Task<bool> StoreCvUrlAsync(Guid roomId, string cvUrl, IFormFile? file, string? useCase = null);
        Task<string?> GetLastCvPdfUrlAsync(Guid roomId);
        Task<AiQuestionExtractionResponse> GetNewQuestionsFromTranscriptAsync(byte[] audioData, Guid roomId, IEnumerable<string>? availableTags = null, string? useCase = null);
        Task<AiCvEvaluationResponseDto?> EvaluateCvAsync(System.IO.Stream stream, string fileName, string contentType, string? useCase = null);
    }
}
