using Asp.Versioning;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> GetList(int userId)
        {
            UserRole role = UserRole.Interviewee;
            var list = await _getRoomHistory.ExecuteAsync(role, userId);

            return Ok(new
            {
                success = true,
                message = "Success",
                data = list
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody]int interviewerId, int intervieweeId)
        {
            int roomId = await _createRoom.ExecuteAsync(intervieweeId);

            return Ok(new
            {
                success = true,
                message = "Success",
                data = new {
                    roomId = roomId
                }
            });
        }
    }
}
