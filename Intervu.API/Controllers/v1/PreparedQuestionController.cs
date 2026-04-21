using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Interfaces.UseCases.PreparedQuestion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    /// <summary>
    /// Coach-only endpoints for building and executing the "Prepared Questions"
    /// roadmap attached to a specific <c>InterviewRoom</c>. All mutations are gated
    /// on the caller being the room's assigned coach (enforced in the use cases).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/prepared-questions")]
    public class PreparedQuestionController : ControllerBase
    {
        private readonly IGetPreparedQuestions _get;
        private readonly IAddCustomPreparedQuestion _addCustom;
        private readonly IAddPreparedQuestionFromBank _addFromBank;
        private readonly IUpdatePreparedQuestion _update;
        private readonly IDeletePreparedQuestion _delete;
        private readonly IReorderPreparedQuestions _reorder;
        private readonly IMarkPreparedQuestionAsked _markAsked;
        private readonly IUnmarkPreparedQuestionAsked _unmarkAsked;
        private readonly ISendPreparedQuestionToEditor _sendToEditor;

        public PreparedQuestionController(
            IGetPreparedQuestions get,
            IAddCustomPreparedQuestion addCustom,
            IAddPreparedQuestionFromBank addFromBank,
            IUpdatePreparedQuestion update,
            IDeletePreparedQuestion delete,
            IReorderPreparedQuestions reorder,
            IMarkPreparedQuestionAsked markAsked,
            IUnmarkPreparedQuestionAsked unmarkAsked,
            ISendPreparedQuestionToEditor sendToEditor)
        {
            _get = get;
            _addCustom = addCustom;
            _addFromBank = addFromBank;
            _update = update;
            _delete = delete;
            _reorder = reorder;
            _markAsked = markAsked;
            _unmarkAsked = unmarkAsked;
            _sendToEditor = sendToEditor;
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpGet("rooms/{roomId}")]
        public async Task<IActionResult> GetByRoom(Guid roomId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _get.ExecuteAsync(roomId, userId);
            return Ok(new { success = true, message = "Success", data });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPost("rooms/{roomId}/custom")]
        public async Task<IActionResult> AddCustom(
            Guid roomId,
            [FromBody] CreateCustomPreparedQuestionRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _addCustom.ExecuteAsync(roomId, request, userId);
            return Ok(new { success = true, message = "Prepared question added", data });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPost("rooms/{roomId}/from-bank")]
        public async Task<IActionResult> AddFromBank(
            Guid roomId,
            [FromBody] ImportBankQuestionRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _addFromBank.ExecuteAsync(roomId, request, userId);
            return Ok(new { success = true, message = "Bank question imported", data });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{preparedQuestionId}")]
        public async Task<IActionResult> Update(
            Guid preparedQuestionId,
            [FromBody] UpdatePreparedQuestionRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _update.ExecuteAsync(preparedQuestionId, request, userId);
            return Ok(new { success = true, message = "Prepared question updated", data });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpDelete("{preparedQuestionId}")]
        public async Task<IActionResult> Delete(Guid preparedQuestionId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            await _delete.ExecuteAsync(preparedQuestionId, userId);
            return Ok(new { success = true, message = "Prepared question removed" });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("rooms/{roomId}/reorder")]
        public async Task<IActionResult> Reorder(
            Guid roomId,
            [FromBody] ReorderPreparedQuestionsRequest request)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            await _reorder.ExecuteAsync(roomId, request, userId);
            return Ok(new { success = true, message = "Prepared questions reordered" });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{preparedQuestionId}/mark-asked")]
        public async Task<IActionResult> MarkAsked(Guid preparedQuestionId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _markAsked.ExecuteAsync(preparedQuestionId, userId);
            return Ok(new { success = true, message = "Marked as asked", data });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{preparedQuestionId}/unmark-asked")]
        public async Task<IActionResult> UnmarkAsked(Guid preparedQuestionId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _unmarkAsked.ExecuteAsync(preparedQuestionId, userId);
            return Ok(new { success = true, message = "Unmarked", data });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{preparedQuestionId}/send-to-editor")]
        public async Task<IActionResult> SendToEditor(Guid preparedQuestionId)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user credentials" });
            }

            var data = await _sendToEditor.ExecuteAsync(preparedQuestionId, userId);
            return Ok(new { success = true, message = "Sent to editor", data });
        }

        private bool TryGetUserId(out Guid userId)
        {
            return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }
    }
}
