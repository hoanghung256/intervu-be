using Asp.Versioning;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.Interfaces.UseCases.Availability;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1.InterviewerAvailability
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IGetInterviewerAvailabilities _getInterviewerAvailabilities;
        private readonly ICreateInterviewerAvailability _createInterviewerAvailability;

        public AvailabilitiesController(IGetInterviewerAvailabilities getInterviewerAvailabilities, ICreateInterviewerAvailability createInterviewerAvailability)
        {
            _getInterviewerAvailabilities = getInterviewerAvailabilities;
            _createInterviewerAvailability = createInterviewerAvailability;
        }
        [HttpGet("{interviewerId}")]
        public async Task<IActionResult> GetInterviewerAvailabilities(int interviewerId, [FromQuery] int month = 0, [FromQuery] int year = 0)
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
    }
}
