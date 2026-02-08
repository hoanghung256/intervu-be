using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Intervu.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InterviewRoomController : Controller
    {
        private readonly IGetRoomHistory _getRoomHistory;

        public InterviewRoomController(IGetRoomHistory getRoomHistory)
        {
            _getRoomHistory = getRoomHistory;
        }

        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            _ = Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out UserRole role);

            var list = await _getRoomHistory.ExecuteAsync(role, userId);

            return Ok(new
            {
                success = true, 
                message = "Success",
                data = list
            });
        }
    }
}
