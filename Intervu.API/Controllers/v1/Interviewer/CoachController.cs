using Asp.Versioning;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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
        public async Task<IActionResult> GetAllCoach(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 24,
            [FromQuery] Guid? companyId = null,
            [FromQuery] Guid? skillId = null,
            [FromQuery] string? skillIds = null,
            [FromQuery] string? levels = null,
            [FromQuery] int? minExperienceYears = null,
            [FromQuery] int? maxExperienceYears = null,
            [FromQuery] int? minPrice = null,
            [FromQuery] int? maxPrice = null,
            [FromQuery] string? searchTerm = "")
        {
            var parsedSkillIds = ParseGuidList(skillIds);
            var parsedLevels = ParseStringList(levels);

            var request = new GetCoachFilterRequest
            {
                Search = searchTerm,
                CompanyId = companyId,
                SkillId = skillId,
                SkillIds = parsedSkillIds,
                Levels = parsedLevels,
                MinExperienceYears = minExperienceYears,
                MaxExperienceYears = maxExperienceYears,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
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

        private static List<Guid>? ParseGuidList(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return null;
            }

            var ids = csv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .ToList();

            return ids.Count == 0 ? null : ids;
        }

        private static List<string>? ParseStringList(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return null;
            }

            var values = csv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return values.Count == 0 ? null : values;
        }
    }
}
