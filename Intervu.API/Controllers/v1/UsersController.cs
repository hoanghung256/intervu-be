using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IGetCurrentUserProfile _getCurrentUserProfile;

        public UsersController(IGetCurrentUserProfile getCurrentUserProfile)
        {
            _getCurrentUserProfile = getCurrentUserProfile;
        }

        [HttpGet("me")]
        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        public async Task<IActionResult> GetMe()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid access token",
                    data = (object?)null,
                });
            }

            var result = await _getCurrentUserProfile.ExecuteAsync(userId);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found",
                    data = (object?)null,
                });
            }

            return Ok(new
            {
                success = true,
                message = "Success",
                data = result,
            });
        }
    }
}