using Asp.Versioning;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.Services;
using Intervu.Application.Interfaces.UseCases.Assessment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/assessment")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _service;
        private readonly ISaveAssessmentAnswersUseCase _saveAssessmentAnswersUseCase;

        public AssessmentController(
            IAssessmentService service,
            ISaveAssessmentAnswersUseCase saveAssessmentAnswersUseCase)
        {
            _service = service;
            _saveAssessmentAnswersUseCase = saveAssessmentAnswersUseCase;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessSurvey([FromBody] SurveyResponsesDto request)
        {
            var result = await _service.ProcessSurveyResponsesAsync(request);
            return Ok(result);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserAssessment([FromRoute] Guid userId)
        {
            var snapshot = await _service.GetUserSkillAssessmentSnapshotAsync(userId);
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
        public async Task<IActionResult> GenerateRoadmap([FromBody] GenerateRoadmapFromSurveyRequestDto request)
        {
            try
            {
                var result = await _service.GenerateRoadmapFromSurveyAsync(request.UserId, request.ForceRegenerate);

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
        }

        [HttpGet("roadmap/{userId:guid}")]
        public async Task<IActionResult> GetRoadmap(Guid userId)
        {
            var roadmap = await _service.GetRoadmapByUserIdAsync(userId);

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

        [HttpPost("answers")]
        public async Task<IActionResult> SaveAnswers([FromBody] SaveAssessmentAnswersRequestDto request)
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
