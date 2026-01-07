using Asp.Versioning;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Intervu.API.Controllers.v1.InterviewerAvailability
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IGetInterviewerAvailabilities _getInterviewerAvailabilities;
        private readonly ICreateInterviewerAvailability _createInterviewerAvailability;
        private readonly IDeleteInterviewerAvailability _deleteInterviewerAvailability;
        private readonly IUpdateInterviewerAvailability _updateInterviewerAvailability;

        public AvailabilitiesController(IGetInterviewerAvailabilities getInterviewerAvailabilities, ICreateInterviewerAvailability createInterviewerAvailability, IDeleteInterviewerAvailability deleteInterviewerAvailability, IUpdateInterviewerAvailability updateInterviewerAvailability)
        {
            _getInterviewerAvailabilities = getInterviewerAvailabilities;
            _createInterviewerAvailability = createInterviewerAvailability;
            _deleteInterviewerAvailability = deleteInterviewerAvailability;
            _updateInterviewerAvailability = updateInterviewerAvailability;
        }
        [HttpGet("{interviewerId}")]
        public async Task<IActionResult> GetInterviewerAvailabilities(Guid interviewerId, [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            var data = await _getInterviewerAvailabilities.ExecuteAsync(interviewerId, month, year);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateInterviewerAvailability([FromBody] InterviewerAvailabilityCreateDto request)
        {
            try
            {
                var id = await _createInterviewerAvailability.ExecuteAsync(request);
                return Ok(new { success = true, message = "Created", data = new { id } });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{availabilityId}")]
        public async Task<IActionResult> DeleteInterviewerAvailability(Guid availabilityId)
        {
            try
            {
                await _deleteInterviewerAvailability.ExecuteAsync(availabilityId);
                return Ok(new { success = true, message = "Deleted" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{availabilityId}")]
        public async Task<IActionResult> UpdateInterviewerAvailability(Guid availabilityId, [FromBody] InterviewerAvailabilityUpdateDto request)
        {
            try
            {
                await _updateInterviewerAvailability.ExecuteAsync(availabilityId, request);
                return Ok(new { success = true, message = "Updated" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

