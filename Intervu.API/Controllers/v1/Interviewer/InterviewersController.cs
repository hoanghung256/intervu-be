using Asp.Versioning;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1.Interviewer
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InterviewersController : ControllerBase
    {
        IGetAllInterviewers _getAllInterviewers;
        public InterviewersController(IGetAllInterviewers getAllInterviewers)
        {
            _getAllInterviewers = getAllInterviewers;
        }

        // [GET] api/interviewers?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAllInterviewers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _getAllInterviewers.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = pagedResult
            });
        }
    }
}
