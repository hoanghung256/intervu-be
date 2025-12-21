using Asp.Versioning;
using AutoMapper;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Interviewer;
using Intervu.Application.Interfaces.UseCases.InterviewerProfile;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Intervu.API.Controllers.v1.Interviewer
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/interviewer-profile")]
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
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnInterviewerProfile([FromRoute] Guid id)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getInterviewProfile.ViewOwnProfileAsync(id);
                return Ok(new
                {
                    success = true,
                    message = msg,
                    data = profile
                });
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                success = true,
                message = msg,
            });
        }

        //[GET] api/interviewerprofile/interviewee/{id}/profile
        //[Authorize(Policy = AuthorizationPolicies.Interviewee)]
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfileByInterviewee([FromRoute] Guid id)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getInterviewProfile.ViewProfileForIntervieweeAsync(id);
                return Ok(new
                {
                    success = true,
                    message = msg,
                    data = profile
                });
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                success = true,
                message = msg,
            });
        }

        //[POST] api/interviewerprofile
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateInterviewerProfile([FromBody] InterviewerCreateDto request)
        {
            
            string msg = "Profile created successfully";
            try
            {
                await _createInterviewProfile.CreateInterviewRequest(request);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                success = true,
                message = msg
            });
        }

        // [PUT] api/interviewer-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInterviewerProfile([FromRoute] Guid id, [FromBody] InterviewerUpdateDto request)
        {
            string msg = "Profile update successfully!";
            try
            {
                InterviewerProfileDto? profile = await _getInterviewProfile.ViewOwnProfileAsync(id);
                profile = await _updateInterviewProfile.UpdateInterviewProfile(id, request);
                return Ok(new
                {
                    success = true,
                    message = "Profile updated successfully"
                });
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                success = true,
                message = msg
            });
        }

        // [PUT] api/interviewer-profile/{id}/status
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateInterviewerStatus([FromRoute] Guid id, [FromBody] InterviewerProfileStatus status)
        {
            string msg = "Profile status update successfully";
            try
            {
                InterviewerViewDto profile = await _updateInterviewProfile.UpdateInterviewStatus(id, status);
                return Ok(new
                {
                    success = true,
                    message = "Status updated successfully"
                });
            }
            catch(Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                success = true,
                message = msg
            });
        }

        // [DELETE] api/interviewer-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInterviewerProfile([FromRoute] Guid id)
        {
            string msg = "Profile deleted successfully";
            try
            {
                await _deleteInterviewerProfile.DeleteInterviewProfile(id);
            } catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Ok(new
            {
                success = true,
                message = msg
            });
        }
    }
}
