using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1.Interviewer
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewerProfileController : ControllerBase
    {
        private readonly ICreateInterviewProfile _createInterviewProfile;
        private readonly IUpdateInterviewProfile _updateInterviewProfile;
        private readonly IViewInterviewProfile _getInterviewProfile;
        private readonly IDeleteInterviewerProfile _deleteInterviewerProfile;

        public InterviewerProfileController(ICreateInterviewProfile createInterviewProfile, IUpdateInterviewProfile updateInterviewProfile, IViewInterviewProfile getInterviewProfile, IDeleteInterviewerProfile deleteInterviewerProfile)
        {
            _createInterviewProfile = createInterviewProfile;
            _updateInterviewProfile = updateInterviewProfile;
            _getInterviewProfile = getInterviewProfile;
            _deleteInterviewerProfile = deleteInterviewerProfile;
        }

        //[GET] api/interviewerprofile/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnInterviewerProfile([FromRoute] int id)
        {
            var profile = await _getInterviewProfile.ViewOwnProfileAsync(id);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = profile
            });
        }

        //[GET] api/interviewerprofile/interviewee/{id}/profile
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByInterviewee([FromRoute] int id)
        {
            var profile = await _getInterviewProfile.ViewProfileForIntervieweeAsync(id);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = profile
            });
        }

        //[POST] api/interviewerprofile
        [HttpPost]
        public async Task<IActionResult> CreateInterviewerProfile([FromBody] InterviewerCreateDto request)
        {
            await _createInterviewProfile.CreateInterviewRequest(request);
            return Ok(new
            {
                success = true,
                message = "Profile created successfully",

            });
        }

        // [PUT] api/interviewerprofile/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInterviewerProfile([FromRoute] int id, [FromBody] InterviewerUpdateDto request)
        {
            InterviewerProfileDto profile = await _getInterviewProfile.ViewOwnProfileAsync(id);
            if (profile == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Id in URL does not match Id in body"
                });
            }

            profile = await _updateInterviewProfile.UpdateInterviewProfile(id, request);
            return Ok(new
            {
                success = true,
                message = "Profile updated successfully"
            });
        }

        // [PUT] api/interviewerprofile/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateInterviewerStatus([FromRoute] int id, [FromBody] InterviewerProfileStatus status)
        {
            InterviewerViewDto profile = await _updateInterviewProfile.UpdateInterviewStatus(id, status);
            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Interviewer profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Status updated successfully"
            });
        }

        // [DELETE] api/interviewerprofile/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInterviewerProfile([FromRoute] int id)
        {
            var profile = await _getInterviewProfile.ViewOwnProfileAsync(id);
            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Interviewer profile not found"
                });
            }

            _deleteInterviewerProfile.DeleteInterviewerProfileAsync(id);
            return Ok(new
            {
                success = true,
                message = "Profile deleted successfully"
            });
        }
    }
}
