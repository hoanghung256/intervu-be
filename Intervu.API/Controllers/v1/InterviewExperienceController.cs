using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
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
    [Route("api/v{version:apiVersion}/interview-experiences")]
    public class InterviewExperienceController : ControllerBase
    {
        private readonly IGetInterviewExperiences _getList;
        private readonly IGetInterviewExperienceDetail _getDetail;
        private readonly ICreateInterviewExperience _create;
        private readonly IUpdateInterviewExperience _update;
        private readonly IDeleteInterviewExperience _delete;
        private readonly IAddQuestion _addQuestion;

        public InterviewExperienceController(
            IGetInterviewExperiences getList,
            IGetInterviewExperienceDetail getDetail,
            ICreateInterviewExperience create,
            IUpdateInterviewExperience update,
            IDeleteInterviewExperience delete,
            IAddQuestion addQuestion)
        {
            _getList = getList;
            _getDetail = getDetail;
            _create = create;
            _update = update;
            _delete = delete;
            _addQuestion = addQuestion;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] InterviewExperienceFilterRequest filter)
        {
            var result = await _getList.ExecuteAsync(filter);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _getDetail.ExecuteAsync(id);
            if (result == null) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInterviewExperienceRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            var id = await _create.ExecuteAsync(request, userId);
            return Ok(new { success = true, message = "Interview experience submitted successfully", data = id });
        }

        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInterviewExperienceRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _update.ExecuteAsync(id, request, userId);
            return Ok(new { success = true, message = "Updated successfully" });
        }

        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _delete.ExecuteAsync(id, userId);
            return Ok(new { success = true, message = "Deleted successfully" });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("{experienceId:guid}/questions")]
        public async Task<IActionResult> AddQuestion(Guid experienceId, [FromBody] CreateQuestionRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            var result = await _addQuestion.ExecuteAsync(experienceId, request, userId);
            var message = result.IsLinked
                ? "Answer posted as comment on existing question"
                : "Question added";
            return Ok(new { success = true, message, data = result });
        }
    }
}
