using Asp.Versioning;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.Interfaces.UseCases.InterviewType;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InterviewTypeController : Controller
    {
        private readonly IGetInterviewType _getInterviewType;
        private readonly IUpdateInterviewType _updateInterviewType;
        private readonly ICreateInterviewType _createInterviewType;

        public InterviewTypeController(IGetInterviewType getInterviewType, IUpdateInterviewType updateInterviewType, ICreateInterviewType createInterviewType)
        {
            _getInterviewType = getInterviewType;
            _updateInterviewType = updateInterviewType;
            _createInterviewType = createInterviewType;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _getInterviewType.ExecuteAsync(pageSize, page);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getInterviewType.ExecuteAsync(id);
            return result is not null ? Ok(result) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InterviewTypeDto request)
        {
            await _createInterviewType.ExecuteAsync(request);
            return Ok(request);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InterviewTypeDto request)
        {
            try
            {
                await _updateInterviewType.ExecuteAsync(id, request);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the interview type.");
            }
        }
    }
}
