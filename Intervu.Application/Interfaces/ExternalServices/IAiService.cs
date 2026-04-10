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
        Task<GenerateAssessmentResponse> GenerateAssessmentAsync(GenerateAssessmentRequest request);
        Task<AiGenerateRoadmapResponseDto?> GenerateRoadmapAsync(AiGenerateRoadmapRequestDto request);
        Task<bool> StoreCvUrlAsync(Guid roomId, string cvUrl, IFormFile? file);
        Task<string?> GetLastCvPdfUrlAsync(Guid roomId);
        Task<AiQuestionExtractionResponse> GetNewQuestionsFromTranscriptAsync(byte[] audioData, Guid roomId);
        Task<AiCvEvaluationResponseDto?> EvaluateCvAsync(System.IO.Stream stream, string fileName, string contentType);
    }
}
