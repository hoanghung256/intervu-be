using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/questions")]
    public class QuestionController : ControllerBase
    {
        private readonly IGetQuestionList _getList;
        private readonly IGetQuestionDetail _getDetail;
        private readonly IUpdateQuestion _update;
        private readonly IDeleteQuestion _delete;
        private readonly ISearchQuestions _search;
        private readonly ILikeQuestion _likeQuestion;
        private readonly ISaveQuestion _saveQuestion;
        private readonly IGetSavedQuestions _getSaved;
        private readonly IReportQuestion _reportQuestion;
        private readonly IGetQuestionReports _getQuestionReports;
        private readonly IUpdateQuestionReportStatus _updateReportStatus;
        private readonly IModerateQuestion _moderateQuestion;

        public QuestionController(
            IGetQuestionList getList,
            IGetQuestionDetail getDetail,
            IUpdateQuestion update,
            IDeleteQuestion delete,
            ISearchQuestions search,
            ILikeQuestion likeQuestion,
            ISaveQuestion saveQuestion,
            IGetSavedQuestions getSaved,
            IReportQuestion reportQuestion,
            IGetQuestionReports getQuestionReports,
            IUpdateQuestionReportStatus updateReportStatus,
            IModerateQuestion moderateQuestion)
        {
            _getList = getList;
            _getDetail = getDetail;
            _update = update;
            _delete = delete;
            _search = search;
            _likeQuestion = likeQuestion;
            _saveQuestion = saveQuestion;
            _getSaved = getSaved;
            _reportQuestion = reportQuestion;
            _getQuestionReports = getQuestionReports;
            _updateReportStatus = updateReportStatus;
            _moderateQuestion = moderateQuestion;
        }

        private Guid? GetOptionalUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        private bool TryGetRequiredUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var optionalUserId = GetOptionalUserId();
            if (!optionalUserId.HasValue)
            {
                return false;
            }

            userId = optionalUserId.Value;
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] QuestionFilterRequest filter)
        {
            var userId = GetOptionalUserId();
            var result = await _getList.ExecuteAsync(filter, userId);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword, [FromQuery] int limit = 10)
        {
            var result = await _search.ExecuteAsync(keyword, limit);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [HttpGet("{questionId:guid}")]
        public async Task<IActionResult> GetDetail(Guid questionId)
        {
            var userId = GetOptionalUserId();
            var result = await _getDetail.ExecuteAsync(questionId, userId);
            if (result == null) return NotFound(new { success = false, message = "Question not found" });
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpGet("saved")]
        public async Task<IActionResult> GetSaved()
        {
            if (!TryGetRequiredUserId(out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            var result = await _getSaved.ExecuteAsync(userId);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("{questionId:guid}/like")]
        public async Task<IActionResult> LikeQuestion(Guid questionId)
        {
            if (!TryGetRequiredUserId(out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            var isLiked = await _likeQuestion.ExecuteAsync(questionId, userId);
            var isUpvote = isLiked;
            await _update.VoteAsync(questionId, isUpvote, userId);
            return Ok(new { success = true, message = isLiked ? "Liked" : "Unliked", data = new { isLiked } });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("{questionId:guid}/save")]
        public async Task<IActionResult> SaveQuestion(Guid questionId, [FromBody] bool isSaved)
        {
            if (!TryGetRequiredUserId(out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            var isSavedQuestion = await _saveQuestion.ExecuteAsync(questionId, isSaved, userId);
            return Ok(new { success = true, message = isSaved ? "Saved" : "Unsaved", data = new { isSaved } });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("{questionId:guid}/report")]
        public async Task<IActionResult> ReportQuestion(Guid questionId, [FromBody] ReportQuestionRequest request)
        {
            if (!TryGetRequiredUserId(out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            var result = await _reportQuestion.ExecuteAsync(questionId, request, userId);
            return Ok(new { success = true, message = "Question reported", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPut("{questionId:guid}")]
        public async Task<IActionResult> Update(Guid questionId, [FromBody] UpdateQuestionRequest request)
        {
            if (!TryGetRequiredUserId(out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            await _update.ExecuteAsync(questionId, request, userId);
            return Ok(new { success = true, message = "Question updated" });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpDelete("{questionId:guid}")]
        public async Task<IActionResult> Delete(Guid questionId)
        {
            if (!TryGetRequiredUserId(out Guid userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            await _delete.ExecuteAsync(questionId, userId);
            return Ok(new { success = true, message = "Question deleted" });
        }

        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] QuestionReportFilterRequest filter)
        {
            var result = await _getQuestionReports.ExecuteAsync(filter);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPut("reports/{reportId:guid}/status")]
        public async Task<IActionResult> UpdateReportStatus(Guid reportId, [FromBody] UpdateQuestionReportStatusRequest request)
        {
            if (!TryGetRequiredUserId(out Guid adminUserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }
            await _updateReportStatus.ExecuteAsync(reportId, request, adminUserId);
            return Ok(new { success = true, message = "Report status updated" });
        }

        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPost("{questionId:guid}/moderate")]
        public async Task<IActionResult> Moderate(Guid questionId, [FromBody] ModerateQuestionRequest request)
        {
            if (!TryGetRequiredUserId(out Guid adminUserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }
            await _moderateQuestion.ExecuteAsync(questionId, request.Status, adminUserId);
            return Ok(new { success = true, message = "Question moderated" });
        }

        //[HttpPost("{questionId:guid}/vote")]
        //public async Task<IActionResult> Vote(Guid questionId, [FromQuery] bool isUpvote)
        //{
        //    _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
        //    await _update.VoteAsync(questionId, isUpvote, userId);
        //    return Ok(new { success = true, message = "Vote recorded" });
        //}
    }
}
