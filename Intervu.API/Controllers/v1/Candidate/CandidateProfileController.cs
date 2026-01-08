using Asp.Versioning;
using AutoMapper;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Intervu.Application.DTOs.Candidate;

namespace Intervu.API.Controllers.v1.Candidate
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/candidate-profile")]
    public class CandidateProfileController : ControllerBase
    {
        private readonly ICreateCandidateProfile _createCandidateProfile;
        private readonly IUpdateCandidateProfile _updateCandidateProfile;
        private readonly IViewCandidateProfile _getCandidateProfile;
        private readonly IDeleteCandidateProfile _deleteCandidateProfile;

        public CandidateProfileController(ICreateCandidateProfile createCandidateProfile, IUpdateCandidateProfile updateCandidateProfile, IViewCandidateProfile getCandidateProfile, IDeleteCandidateProfile deleteCandidateProfile)
        {
            _createCandidateProfile = createCandidateProfile;
            _updateCandidateProfile = updateCandidateProfile;
            _getCandidateProfile = getCandidateProfile;
            _deleteCandidateProfile = deleteCandidateProfile;
        }

        //[GET] api/Candidate-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnCandidateProfile([FromRoute] Guid id)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getCandidateProfile.ViewOwnProfileAsync(id);
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

        //[GET] api/Candidateprofile/candidate/{id}/profile
        //[Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpGet("{slugProfileUrl}/profile")]
        public async Task<IActionResult> GetProfileByCandidate([FromRoute] string slugProfileUrl)
        {
            string msg = "Get profile successfully!";
            try
            {
                var profile = await _getCandidateProfile.ViewProfileBySlugAsync(slugProfileUrl);
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

        //[POST] api/Candidate-profile
        //[Authorize(Policy = AuthorizationPolicies.Candidate)]
        //[HttpPost]
        //public async Task<IActionResult> CreateCandidateProfile([FromBody] CandidateCreateDto request)
        //{
            
        //    string msg = "Profile created successfully";
        //    try
        //    {
        //        await _createCandidateProfile.CreateCandidateProfileAsync(request);
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

        // [PUT] api/Candidate-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandidateProfile([FromRoute] Guid id, [FromBody] CandidateUpdateDto request)
        {
            string msg = "Profile update successfully!";
            try
            {
                CandidateProfileDto? profile = await _getCandidateProfile.ViewOwnProfileAsync(id);
                profile = await _updateCandidateProfile.UpdateCandidateProfileAsync(id, request);
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

        // [PUT] api/Candidate-profile/{id}/status
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateCandidateStatus([FromRoute] Guid id, [FromBody] UserStatus status)
        {
            string msg = "Profile status update successfully";
            try
            {
                CandidateViewDto profile = await _updateCandidateProfile.UpdateCandidateStatusAsync(id, status);
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

        // [DELETE] api/Candidate-profile/{id}
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidateProfile([FromRoute] Guid id)
        {
            string msg = "Profile deleted successfully";
            try
            {
                await _deleteCandidateProfile.DeleteCandidateProfileAsync(id);
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
