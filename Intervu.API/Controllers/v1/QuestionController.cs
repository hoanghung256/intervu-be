using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public QuestionController(
            IGetQuestionList getList,
            IGetQuestionDetail getDetail,
            IUpdateQuestion update,
            IDeleteQuestion delete)
        {
            _getList = getList;
            _getDetail = getDetail;
            _update = update;
            _delete = delete;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] QuestionFilterRequest filter)
        {
            var result = await _getList.ExecuteAsync(filter);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [HttpGet("{questionId:guid}")]
        public async Task<IActionResult> GetDetail(Guid questionId)
        {
            var result = await _getDetail.ExecuteAsync(questionId);
            if (result == null) return NotFound(new { success = false, message = "Question not found" });
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPut("{questionId:guid}")]
        public async Task<IActionResult> Update(Guid questionId, [FromBody] UpdateQuestionRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _update.ExecuteAsync(questionId, request, userId);
            return Ok(new { success = true, message = "Question updated" });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpDelete("{questionId:guid}")]
        public async Task<IActionResult> Delete(Guid questionId)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _delete.ExecuteAsync(questionId, userId);
            return Ok(new { success = true, message = "Question deleted" });
        }
    }
}
