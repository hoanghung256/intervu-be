using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IGetCurrentRoom _getCurrentRoom;
        private readonly IGetCoachEvaluation _getCoachEvaluation;
        private readonly ISubmitCoachEvaluation _submitCoachEvaluation;
        private readonly ISaveCoachEvaluationDraft _saveCoachEvaluationDraft;

        public InterviewRoomController(
            IGetRoomHistory getRoomHistory,
            IGetCurrentRoom getCurrentRoom,
            IGetCoachEvaluation getCoachEvaluation,
            ISubmitCoachEvaluation submitCoachEvaluation,
            ISaveCoachEvaluationDraft saveCoachEvaluationDraft)
        {
            _getRoomHistory = getRoomHistory;
            _getCurrentRoom = getCurrentRoom;
            _getCoachEvaluation = getCoachEvaluation;
            _submitCoachEvaluation = submitCoachEvaluation;
            _saveCoachEvaluationDraft = saveCoachEvaluationDraft;
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
                data = result
            });
        }

        /// <summary>
        /// Get a single interview room by ID
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var room = await _getCurrentRoom.ExecuteAsync(id);

            if (room == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Interview room not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Success",
                data = room
            });
        }

        /// <summary>
        /// Get evaluation form and current answers for a completed interview (coach only)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
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

        /// <summary>
        /// Save coach evaluation draft for a completed interview
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPatch("{interviewRoomId}/coach-evaluation/draft")]
        public async Task<IActionResult> SaveCoachEvaluationDraft(
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

            await _saveCoachEvaluationDraft.ExecuteAsync(
                interviewRoomId,
                userId,
                request?.Results ?? new List<EvaluationResultDto>());

            return Ok(new
            {
                success = true,
                message = "Evaluation draft saved successfully"
            });
        }
    }
}
