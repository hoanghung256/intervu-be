using Asp.Versioning;
using AutoMapper;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Interviewee;
using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Intervu.API.Controllers.v1.Interviewee
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/interviewee-profile")]
    public class IntervieweeProfileController : ControllerBase
    {
        private readonly ICreateIntervieweeProfile _createIntervieweeProfile;
        private readonly IUpdateIntervieweeProfile _updateIntervieweeProfile;
        private readonly IViewIntervieweeProfile _getIntervieweeProfile;
        private readonly IDeleteIntervieweeProfile _deleteIntervieweeProfile;

        public IntervieweeProfileController(ICreateIntervieweeProfile createIntervieweeProfile, IUpdateIntervieweeProfile updateIntervieweeProfile, IViewIntervieweeProfile getIntervieweeProfile, IDeleteIntervieweeProfile deleteIntervieweeProfile)
        {
            _createIntervieweeProfile = createIntervieweeProfile;
            _updateIntervieweeProfile = updateIntervieweeProfile;
            _getIntervieweeProfile = getIntervieweeProfile;
            _deleteIntervieweeProfile = deleteIntervieweeProfile;
        }

        //[GET] api/Intervieweeprofile/{id}
        [Authorize(Policy = AuthorizationPolicies.Interviewee)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnIntervieweeProfile([FromRoute] Guid id)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getIntervieweeProfile.ViewOwnProfileAsync(id);
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

        //[GET] api/Intervieweeprofile/interviewee/{id}/profile
        //[Authorize(Policy = AuthorizationPolicies.Interviewee)]
        [HttpGet("{slugProfileUrl}/profile")]
        public async Task<IActionResult> GetProfileByInterviewee([FromRoute] string slugProfileUrl)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getIntervieweeProfile.ViewProfileBySlugAsync(slugProfileUrl);
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

        //[POST] api/Intervieweeprofile
        //[Authorize(Policy = AuthorizationPolicies.Interviewee)]
        //[HttpPost]
        //public async Task<IActionResult> CreateIntervieweeProfile([FromBody] IntervieweeCreateDto request)
        //{
            
        //    string msg = "Profile created successfully";
        //    try
        //    {
        //        await _createIntervieweeProfile.CreateIntervieweeProfileAsync(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        msg = ex.Message;
        //    }
        //    return Ok(new
        //    {
        //        success = true,
        //        message = msg
        //    });
        //}

        // [PUT] api/Interviewee-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Interviewee)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIntervieweeProfile([FromRoute] Guid id, [FromBody] IntervieweeUpdateDto request)
        {
            string msg = "Profile update successfully!";
            try
            {
                IntervieweeProfileDto? profile = await _getIntervieweeProfile.ViewOwnProfileAsync(id);
                profile = await _updateIntervieweeProfile.UpdateIntervieweeProfileAsync(id, request);
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

        // [PUT] api/Interviewee-profile/{id}/status
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateIntervieweeStatus([FromRoute] Guid id, [FromBody] UserStatus status)
        {
            string msg = "Profile status update successfully";
            try
            {
                IntervieweeViewDto profile = await _updateIntervieweeProfile.UpdateIntervieweeStatusAsync(id, status);
                return Ok(new
                {
                    success = true,
                    message = "Status updated successfully"
                });
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

        // [DELETE] api/Interviewee-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIntervieweeProfile([FromRoute] Guid id)
        {
            string msg = "Profile deleted successfully";
            try
            {
                await _deleteIntervieweeProfile.DeleteIntervieweeProfileAsync(id);
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
