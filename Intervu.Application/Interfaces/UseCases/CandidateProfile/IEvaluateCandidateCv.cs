using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Ai;

namespace Intervu.Application.Interfaces.UseCases.CandidateProfile
{
    public interface IEvaluateCandidateCv
    {
        Task<AiCvEvaluationResponseDto?> ExecuteAsync(Guid candidateId, IFormFile? file = null);
    }
}
