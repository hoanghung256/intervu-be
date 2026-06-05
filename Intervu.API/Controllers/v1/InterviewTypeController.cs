using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.Interfaces.UseCases.InterviewType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> GetAll(
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, int.MaxValue)] int pageSize = 10)
        {
            var result = await _getInterviewType.ExecuteAsync(pageSize, page, includeAllStatuses: false);
            return Ok(new {
                success = true,
                message = "Interview types retrieved successfully",
                data = result
            });
        }

        /// <summary>
        /// Admin: all interview types regardless of status (Draft, Active, Inactive, Deprecated).
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllForAdmin(
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, int.MaxValue)] int pageSize = 10)
        {
            var result = await _getInterviewType.ExecuteAsync(pageSize, page, includeAllStatuses: true);
            return Ok(new
            {
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InterviewTypeDto request)
        {
            await _updateInterviewType.ExecuteAsync(id, request);
            return Ok(new { success = true, message = "Interview type updated successfully" });
        }

        [HttpDelete("{id}")]
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
