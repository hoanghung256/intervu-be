using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.InterviewRoom;
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

        /// <summary>
        /// Get list of interview rooms with reschedule status
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GetInterviewRoomsRequestDto request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            _ = Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out UserRole role);

            if (!isGetUserIdSuccess || !isGetRoleSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid user credentials"
                });
            }

            var result = await _getRoomHistory.ExecuteWithPaginationAsync(role, userId, request);

            return Ok(new
            {
                success = true,
                message = "Success",
                data = result.Items
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateInterviewRoomDto createRoomDto)
        {
            Guid roomId = createRoomDto.CoachId == null 
                ? await _createRoom.ExecuteAsync(createRoomDto.CandidateId) 
                : await _createRoom.ExecuteAsync(
                    createRoomDto.CandidateId, 
                    createRoomDto.CoachId.Value, 
                    createRoomDto.ScheduledTime ?? DateTime.UtcNow.AddDays(1));

            return Ok(new
            {
                success = true,
                message = "Interview room created successfully",
                data = new
                {
                    roomId = roomId
                }
            });
        }
    }
}
