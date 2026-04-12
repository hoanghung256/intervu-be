using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.Interfaces.UseCases.GeneratedQuestion;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/generated-questions")]
    public class GeneratedQuestionController : ControllerBase
    {
        private readonly IGetGeneratedQuestionsByRoom _getByRoom;
        private readonly IApproveGeneratedQuestion _approve;
        private readonly IRejectGeneratedQuestion _reject;
        private readonly ICreateGeneratedQuestion _create;

        public GeneratedQuestionController(
            IGetGeneratedQuestionsByRoom getByRoom,
            IApproveGeneratedQuestion approve,
            IRejectGeneratedQuestion reject,
            ICreateGeneratedQuestion create)
        {
            _getByRoom = getByRoom;
            _approve = approve;
            _reject = reject;
            _create = create;
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpGet("rooms/{roomId:guid}")]
        public async Task<IActionResult> GetByRoom(Guid roomId, [FromQuery] GeneratedQuestionStatus? status)
        {
            var result = await _getByRoom.ExecuteAsync(roomId, status);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.InterviewOrAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGeneratedQuestionRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            var id = await _create.ExecuteAsync(request, userId);
            return Ok(new { success = true, message = "Custom question created", data = new { id } });
        }



        [Authorize(Policy = AuthorizationPolicies.InterviewOrAdmin)]
        [HttpPut("{generatedQuestionId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid generatedQuestionId, [FromBody] ApproveGeneratedQuestionRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            var questionId = await _approve.ExecuteAsync(generatedQuestionId, request, userId);
            return Ok(new { success = true, message = "Generated question approved", data = new { questionId } });
        }

        [Authorize(Policy = AuthorizationPolicies.InterviewOrAdmin)]
        [HttpPut("{generatedQuestionId:guid}/reject")]
        public async Task<IActionResult> Reject(Guid generatedQuestionId)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _reject.ExecuteAsync(generatedQuestionId, userId);
            return Ok(new { success = true, message = "Generated question rejected" });
        }
    }
}
