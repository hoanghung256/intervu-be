using Asp.Versioning;
using AutoMapper;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.Interfaces.UseCases.CoachProfile;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Intervu.API.Controllers.v1.Interviewer
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/coach-profile")]
    public class CoachProfileController : ControllerBase
    {
        private readonly ICreateCoachProfile _createCoachProfile;
        private readonly IUpdateCoachProfile _updateCoachProfile;
        private readonly IViewCoachProfile _getCoachProfile;
        private readonly IDeleteCoachProfile _deleteCoachProfile;

        public CoachProfileController(ICreateCoachProfile createCoachProfile, IUpdateCoachProfile updateCoachProfile, IViewCoachProfile getCoachProfile, IDeleteCoachProfile deleteCoachProfile)
        {
            _createCoachProfile = createCoachProfile;
            _updateCoachProfile = updateCoachProfile;
            _getCoachProfile = getCoachProfile;
            _deleteCoachProfile = deleteCoachProfile;
        }

        //[GET] api/coach-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnInterviewerProfile([FromRoute] Guid id)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getCoachProfile.ViewOwnProfileAsync(id);
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

        //[GET] api/coach-profile/{id}/profile
        //[Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpGet("{slugProfileUrl}/profile")]
        public async Task<IActionResult> GetProfileByCandidate([FromRoute] string slugProfileUrl)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getCoachProfile.ViewProfileForCandidateAsync(slugProfileUrl);
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

        //[POST] api/coach-profile
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateInterviewerProfile([FromBody] CoachCreateDto request)
        {
            
            string msg = "Profile created successfully";
            try
            {
                await _createCoachProfile.CreateCoachRequest(request);
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

        // [PUT] api/coach-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoachProfile([FromRoute] Guid id, [FromBody] CoachUpdateDto request)
        {
            string msg = "Profile update successfully!";
            try
            {
                CoachProfileDto? profile = await _getCoachProfile.ViewOwnProfileAsync(id);
                profile = await _updateCoachProfile.ExecuteAsync(id, request);
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
        
        // [PUT] api/coach-profile/{id}/status
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateCoachStatus([FromRoute] Guid id, [FromBody] CoachProfileStatus status)
        {
            string msg = "Profile status update successfully";
            try
            {
                CoachViewDto profile = await _updateCoachProfile.UpdateCoachStatus(id, status);
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

        // [DELETE] api/coach-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoachProfile([FromRoute] Guid id)
        {
            string msg = "Profile deleted successfully";
            try
            {
                await _deleteCoachProfile.ExecuteAsync(id);
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
