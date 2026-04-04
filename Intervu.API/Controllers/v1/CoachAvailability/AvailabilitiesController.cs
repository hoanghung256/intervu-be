using Asp.Versioning;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Intervu.API.Controllers.v1.CoachAvailability
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IGetCoachAvailabilities _getCoachAvailabilities;
        private readonly IGetCoachFreeSlots _getCoachFreeSlots;
        private readonly ICreateCoachAvailability _createCoachAvailability;
        private readonly IDeleteCoachAvailability _deleteCoachAvailability;
        private readonly IUpdateCoachAvailability _updateCoachAvailability;

        public AvailabilitiesController(
            IGetCoachAvailabilities getCoachAvailabilities,
            IGetCoachFreeSlots getCoachFreeSlots,
            ICreateCoachAvailability createCoachAvailability,
            IDeleteCoachAvailability deleteCoachAvailability,
            IUpdateCoachAvailability updateCoachAvailability)
        {
            _getCoachAvailabilities = getCoachAvailabilities;
            _getCoachFreeSlots = getCoachFreeSlots;
            _createCoachAvailability = createCoachAvailability;
            _deleteCoachAvailability = deleteCoachAvailability;
            _updateCoachAvailability = updateCoachAvailability;
        }

        [HttpGet("{coachId}")]
        public async Task<IActionResult> GetCoachAvailabilities([FromRoute] Guid coachId, [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            var data = await _getCoachAvailabilities.ExecuteAsync(coachId, month, year);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = data
            });
        }

        [HttpGet("{coachId}/free-slots")]
        public async Task<IActionResult> GetCoachFreeSlots([FromRoute] Guid coachId, [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            var data = await _getCoachFreeSlots.ExecuteAsync(coachId, month, year);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoachAvailability([FromBody] CoachAvailabilityCreateDto request)
        {
            try
            {
                var ids = await _createCoachAvailability.ExecuteAsync(request);
                return Ok(new { success = true, message = "Created", data = new { ids, blockCount = ids.Count } });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCoachAvailability([FromBody] CoachAvailabilityUpdateDto request)
        {
            try
            {
                await _updateCoachAvailability.ExecuteAsync(request);
                return Ok(new { success = true, message = "Updated" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{availabilityId}")]
        public async Task<IActionResult> DeleteCoachAvailability(Guid availabilityId)
        {
            try
            {
                await _deleteCoachAvailability.ExecuteAsync(availabilityId);
                return Ok(new { success = true, message = "Deleted" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("range")]
        public async Task<IActionResult> DeleteCoachAvailabilityRange([FromBody] CoachAvailabilityDeleteDto request)
        {
            try
            {
                await _deleteCoachAvailability.ExecuteRangeAsync(request);
                return Ok(new { success = true, message = "Range deleted" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
