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
        private readonly IDeleteInterviewType _deleteInterviewType;

        public InterviewTypeController(IGetInterviewType getInterviewType, IUpdateInterviewType updateInterviewType, ICreateInterviewType createInterviewType, IDeleteInterviewType deleteInterviewType)
        {
            _getInterviewType = getInterviewType;
            _updateInterviewType = updateInterviewType;
            _createInterviewType = createInterviewType;
            _deleteInterviewType = deleteInterviewType;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _getInterviewType.ExecuteAsync(pageSize, page);
            return Ok(new {
                success = true,
                message = "Interview types retrieved successfully",
                data = result
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getInterviewType.ExecuteAsync(id);
            return result is not null ? Ok(new { success = true, data = result }) : NotFound(new { success = false, message = "Interview type not found" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InterviewTypeDto request)
        {
            await _createInterviewType.ExecuteAsync(request);
            return Ok(new { success = true, message = "Interview type created successfully", data = request });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InterviewTypeDto request)
        {
            try
            {
                await _updateInterviewType.ExecuteAsync(id, request);
                return Ok(new { success = true, message = "Interview type updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteInterviewType(Guid id)
        {
            try
            {
                await _deleteInterviewType.ExecuteAsync(id);
                return Ok(new { success = true, message = "Interview type deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
