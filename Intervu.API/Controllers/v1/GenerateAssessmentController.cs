using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.DTOs;
using Asp.Versioning;
using Intervu.Application.Interfaces.Services;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/generate-assessment")]
    public class GenerateAssessmentController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IGenerateAssessmentCatalogService _catalogService;

        public GenerateAssessmentController(
            IAiService aiService,
            IGenerateAssessmentCatalogService catalogService)
        {
            _aiService = aiService;     
            _catalogService = catalogService;
        }

        [HttpGet("options")]
        public async Task<IActionResult> GetOptions()
        {
            var result = await _catalogService.GetOptionsAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GenerateAssessmentRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            var result = await _aiService.GenerateAssessmentAsync(request);
            return Ok(result);
        }
    }
}
