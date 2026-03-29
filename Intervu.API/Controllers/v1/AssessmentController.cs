using Asp.Versioning;
using Intervu.Application.DTOs.Assessment;
using Intervu.Application.Interfaces.Services;
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

        public AssessmentController(IAssessmentService service)
        {
            _service = service;
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
    }

}
