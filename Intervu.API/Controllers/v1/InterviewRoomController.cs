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
using System.Collections.Generic;

namespace Intervu.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InterviewRoomController : Controller
    {
        private readonly IGetRoomHistory _getRoomHistory;
        private readonly IGetCoachEvaluation _getCoachEvaluation;
        private readonly ISubmitCoachEvaluation _submitCoachEvaluation;

        public InterviewRoomController(
            IGetRoomHistory getRoomHistory,
            IGetCoachEvaluation getCoachEvaluation,
            ISubmitCoachEvaluation submitCoachEvaluation)
        {
            _getRoomHistory = getRoomHistory;
            _getCoachEvaluation = getCoachEvaluation;
            _submitCoachEvaluation = submitCoachEvaluation;
        }

        /// <summary>
        /// Get list of interview rooms with reschedule status
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GetInterviewRoomsRequestDto request)
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            bool isGetRoleSuccess = Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out UserRole role);

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

        /// <summary>
        /// Get evaluation form and current answers for a completed interview (coach only)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpGet("{interviewRoomId}/coach-evaluation")]
        public async Task<IActionResult> GetCoachEvaluation([FromRoute] Guid interviewRoomId)
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

            if (!isGetUserIdSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid user credentials"
                });
            }

            var evaluation = await _getCoachEvaluation.ExecuteAsync(interviewRoomId, userId);

            return Ok(new
            {
                success = true,
                message = "Success",
                data = evaluation
            });
        }

        /// <summary>
        /// Submit coach evaluation for a completed interview
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPost("{interviewRoomId}/coach-evaluation")]
        public async Task<IActionResult> SubmitCoachEvaluation(
            [FromRoute] Guid interviewRoomId,
            [FromBody] SubmitCoachEvaluationRequest request)
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

            if (!isGetUserIdSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid user credentials"
                });
            }

            await _submitCoachEvaluation.ExecuteAsync(
                interviewRoomId,
                userId,
                request?.Results ?? new List<EvaluationResultDto>());

            return Ok(new
            {
                success = true,
                message = "Evaluation submitted successfully"
            });
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateRoom([FromBody] CreateInterviewRoomDto createRoomDto)
        //{
        //    Guid roomId = createRoomDto.CoachId == null 
        //        ? await _createRoom.ExecuteAsync(createRoomDto.CandidateId) 
        //        : await _createRoom.ExecuteAsync(
        //            createRoomDto.CandidateId, 
        //            createRoomDto.CoachId.Value, 
        //            createRoomDto.ScheduledTime ?? DateTime.UtcNow.AddDays(1));

        //    return Ok(new
        //    {
        //        success = true,
        //        message = "Interview room created successfully",
        //        data = new
        //        {
        //            roomId = roomId
        //        }
        //    });
        //}
    }
}
