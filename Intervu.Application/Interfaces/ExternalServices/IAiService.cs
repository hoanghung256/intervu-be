using System.Threading.Tasks;
using Intervu.Application.DTOs;
using System;
using Intervu.Application.DTOs.Ai;
using Intervu.Application.DTOs.Question;
using Microsoft.AspNetCore.Http;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IAiService
    {
        Task<GenerateAssessmentResponse> GenerateAssessmentAsync(GenerateAssessmentRequest request);
        Task<bool> StoreCvUrlAsync(Guid roomId, string cvUrl, IFormFile? file);
        Task<string?> GetLastCvPdfUrlAsync(Guid roomId);
        Task<AiQuestionExtractionResponse> GetNewQuestionsFromTranscriptAsync(byte[] audioData, Guid roomId);
    }
}
