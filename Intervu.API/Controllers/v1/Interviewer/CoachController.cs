using Asp.Versioning;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Intervu.API.Controllers.v1.Interviewer
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CoachController : ControllerBase
    {
        IGetAllCoach _getAllCoach;
        public CoachController(IGetAllCoach getAllCoach)
        {
            _getAllCoach = getAllCoach;
        }

        // [GET] api/interviewers?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAllCoach([FromQuery] int page = 1, [FromQuery] int pageSize = 24, [FromQuery] Guid? companyId = null, [FromQuery] Guid? skillId = null, [FromQuery] string? searchTerm = "")
        {
            var request = new GetCoachFilterRequest
            {
                Search = searchTerm,
                CompanyId = companyId,
                SkillId = skillId,
                Page = page,
                PageSize = pageSize
            };

            var pagedResult = await _getAllCoach.ExecuteAsync(request);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = pagedResult
            });
        }
    }
}
