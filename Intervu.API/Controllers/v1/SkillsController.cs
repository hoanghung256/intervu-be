using Asp.Versioning;
using Intervu.Application.Interfaces.UseCases.Skill;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SkillsController : Controller
    {
        private readonly IGetAllSkills _getAllSkills;
        public SkillsController(IGetAllSkills getAllSkills)
        {
            _getAllSkills = getAllSkills;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSkills([FromQuery] int page, [FromQuery] int pageSize)
        {
            var skills = await _getAllSkills.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = skills
            });
        }

    }
}

