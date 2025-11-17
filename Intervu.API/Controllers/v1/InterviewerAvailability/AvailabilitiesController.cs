using Asp.Versioning;
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
        public AvailabilitiesController(IGetInterviewerAvailabilities getInterviewerAvailabilities)
        {
            _getInterviewerAvailabilities = getInterviewerAvailabilities;
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
    }
}
