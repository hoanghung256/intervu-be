using Asp.Versioning;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.Services;
using Intervu.Application.Interfaces.UseCases.Assessment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/assessment")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _service;
        private readonly IAiService _aiService;
        private readonly ISaveAssessmentAnswersUseCase _saveAssessmentAnswersUseCase;

        public AssessmentController(
            IAssessmentService service,
            IAiService aiService,
            ISaveAssessmentAnswersUseCase saveAssessmentAnswersUseCase)
        {
            _service = service;
            _aiService = aiService;
            _saveAssessmentAnswersUseCase = saveAssessmentAnswersUseCase;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessSurvey([FromBody] SurveyResponsesDto request, CancellationToken cancellationToken)
        {
            var raw = await _aiService.EvaluateAssessmentRawAsync(
                new EvaluateAssessmentRequestDto
                {
                    Answer = request.Answer ?? new SurveyAnswerJsonDto()
                },
                cancellationToken,
                useCase: "AutoAssessmentEvaluation");

            return Content(raw, "application/json");
        }

        [HttpPost("evaluate-assessment")]
        public async Task<IActionResult> EvaluateAssessment(
            [FromBody] EvaluateAssessmentRequestDto request,
            CancellationToken cancellationToken)
        {
            var raw = await _aiService.EvaluateAssessmentRawAsync(request, cancellationToken, useCase: "AutoAssessmentEvaluation");
            return Content(raw, "application/json");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserAssessment([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var snapshot = await _service.GetUserSkillAssessmentSnapshotAsync(userId, cancellationToken);
            if (snapshot == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                success = true,
                data = snapshot
            });
        }

        [HttpPost("roadmap/generate")]
        public async Task<IActionResult> GenerateRoadmap([FromBody] GenerateRoadmapFromSurveyRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GenerateRoadmapFromSurveyAsync(request.UserId, request.ForceRegenerate, cancellationToken);

                if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new GenerateRoadmapResultDto
                {
                    Status = "failed",
                    Error = ex.Message
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new GenerateRoadmapResultDto
                {
                    Status = "failed",
                    Error = "AI service is temporarily unavailable. Please try again later."
                });
            }
        }

        [HttpGet("roadmap/{userId:guid}")]
        public async Task<IActionResult> GetRoadmap(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var roadmap = await _service.GetRoadmapByUserIdAsync(userId, cancellationToken);

                if (roadmap == null)
                {
                    return NotFound(new GenerateRoadmapResultDto
                    {
                        Status = "failed",
                        Error = "Roadmap not found. Please generate roadmap after finishing survey."
                    });
                }

                return Ok(new GenerateRoadmapResultDto
                {
                    Status = "success",
                    Roadmap = roadmap
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new GenerateRoadmapResultDto
                {
                    Status = "failed",
                    Error = "AI service is temporarily unavailable. Please try again later."
                });
            }
        }

        [HttpPost("answers")]
        public async Task<IActionResult> SaveAnswers([FromBody] SaveAssessmentAnswersRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _saveAssessmentAnswersUseCase.ExecuteAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
