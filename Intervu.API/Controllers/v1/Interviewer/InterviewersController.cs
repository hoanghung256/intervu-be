using Asp.Versioning;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Microsoft.AspNetCore.Mvc;
using System;

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
        public async Task<IActionResult> GetAllInterviewers([FromQuery] int page = 1, [FromQuery] int pageSize = 24, [FromQuery] Guid? companyId = null, [FromQuery] Guid? skillId = null, [FromQuery] string? searchTerm = "")
        {
            var request = new GetInterviewerFilterRequest
            {
                Search = searchTerm,
                CompanyId = companyId,
                SkillId = skillId,
                Page = page,
                PageSize = pageSize
            };

            var pagedResult = await _getAllInterviewers.ExecuteAsync(request);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = pagedResult
            });
        }
    }
}
