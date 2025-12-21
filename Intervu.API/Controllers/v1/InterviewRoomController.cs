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
        private readonly ICreateInterviewRoom _createRoom;

        public InterviewRoomController(IGetRoomHistory getRoomHistory, ICreateInterviewRoom createRoom)
        {
            _getRoomHistory = getRoomHistory;
            _createRoom = createRoom;
        }

        [Authorize(Policy = AuthorizationPolicies.IntervieweeOrInterviewer)]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            bool isGetRoleSuccess = Enum.TryParse<UserRole>(User.FindFirstValue(ClaimTypes.Role), out UserRole role);

            var list = await _getRoomHistory.ExecuteAsync(role, userId);

            return Ok(new
            {
                success = true, 
                message = "Success",
                data = list
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            Guid roomId = createRoomDto.interviewerId == null 
                ? await _createRoom.ExecuteAsync(createRoomDto.intervieweeId) 
                : await _createRoom.ExecuteAsync(createRoomDto.intervieweeId, createRoomDto.interviewerId, DateTime.Now.AddDays(1));

            return Ok(new
            {
                success = true,
                message = "Success",
                data = new
                {
                    roomId = roomId
                }
            });
        }

        public class CreateRoomDto
        {
            public Guid intervieweeId { get; set; }
            public Guid interviewerId { get; set; }
        }
    }
}
