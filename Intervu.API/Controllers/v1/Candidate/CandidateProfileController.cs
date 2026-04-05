using Asp.Versioning;
using AutoMapper;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.Interfaces.UseCases.CandidateProfile;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

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
        private readonly IGetCandidateRating _getCandidateRating;
        private readonly ICandidateProfileRepository _repo;

        public CandidateProfileController(
            ICreateCandidateProfile createCandidateProfile,
            IUpdateCandidateProfile updateCandidateProfile,
            IViewCandidateProfile getCandidateProfile,
            IDeleteCandidateProfile deleteCandidateProfile,
            IGetCandidateRating getCandidateRating,
            ICandidateProfileRepository repo)
        {
            _createCandidateProfile = createCandidateProfile;
            _updateCandidateProfile = updateCandidateProfile;
            _getCandidateProfile = getCandidateProfile;
            _deleteCandidateProfile = deleteCandidateProfile;
            _getCandidateRating = getCandidateRating;
            _repo = repo;
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

        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet("{id}/rating")]
        public async Task<IActionResult> GetCandidateRating([FromRoute] Guid id)
        {
            try
            {
                var rating = await _getCandidateRating.ExecuteAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Candidate rating retrieved successfully",
                    data = new
                    {
                        candidateId = id,
                        rating
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        //[GET] api/Candidateprofile/candidate/{id}/profile
        //[Authorize(Policy = AuthorizationPolicies.Candidate)]
        [AllowAnonymous]
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
            try
            {
                await _updateCandidateProfile.UpdateCandidateProfileAsync(id, request);
                return Ok(new
                {
                    success = true,
                    message = "Profile updated successfully"
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
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

        // [PUT] api/Candidate-profile/{id}/work-experiences
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPut("{id}/work-experiences")]
        public async Task<IActionResult> UpdateCandidateWorkExperiences([FromRoute] Guid id, [FromBody] UpdateCandidateWorkExperiencesRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body is required"
                });
            }

            try
            {
                var profile = await _updateCandidateProfile.UpdateCandidateWorkExperiencesAsync(id, request.WorkExperiences);
                return Ok(new
                {
                    success = true,
                    message = "Work experiences updated successfully",
                    data = profile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        
        // [POST] api/candidate-profile/{profileId}/work-experiences
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPost("{profileId}/work-experiences")]
        public async Task<IActionResult> CreateWorkExperience([FromRoute] Guid profileId, [FromBody] CandidateWorkExperienceDto dto)
        {
            if (dto == null) return BadRequest();

            var entity = new CandidateWorkExperience
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = profileId,
                CompanyName = dto.CompanyName,
                PositionTitle = dto.PositionTitle,
                JobType = dto.JobType,
                Location = dto.Location,
                LocationType = dto.LocationType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrentWorking = dto.IsCurrentWorking,
                IsEnded = dto.IsEnded,
                Description = dto.Description,
                SkillIds = dto.SkillIds ?? new List<Guid>()
            };

            var added = await _repo.AddWorkExperienceAsync(entity);
            return Ok(new { success = true, data = added });
        }

        // [PUT] api/candidate-profile/{profileId}/work-experiences/{workExperienceId}
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPut("{profileId}/work-experiences/{workExperienceId}")]
        public async Task<IActionResult> UpdateWorkExperience([FromRoute] Guid profileId, [FromRoute] Guid workExperienceId, [FromBody] CandidateWorkExperienceDto dto)
        {
            if (dto == null) return BadRequest();

            var entity = new CandidateWorkExperience
            {
                Id = workExperienceId,
                CandidateProfileId = profileId,
                CompanyName = dto.CompanyName,
                PositionTitle = dto.PositionTitle,
                JobType = dto.JobType,
                Location = dto.Location,
                LocationType = dto.LocationType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrentWorking = dto.IsCurrentWorking,
                IsEnded = dto.IsEnded,
                Description = dto.Description,
                SkillIds = dto.SkillIds ?? new List<Guid>()
            };

            await _repo.UpdateWorkExperienceAsync(entity);
            return Ok(new { success = true });
        }

        // [DELETE] api/candidate-profile/{profileId}/work-experiences/{workExperienceId}
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpDelete("{profileId}/work-experiences/{workExperienceId}")]
        public async Task<IActionResult> DeleteWorkExperience([FromRoute] Guid profileId, [FromRoute] Guid workExperienceId)
        {
            await _repo.DeleteWorkExperienceAsync(workExperienceId);
            return Ok(new { success = true });
        }

        // [POST] api/candidate-profile/{profileId}/certificates
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPost("{profileId}/certificates")]
        public async Task<IActionResult> AddCertificate([FromRoute] Guid profileId, [FromBody] CandidateCertificateDto dto)
        {
            if (dto == null) return BadRequest();

            var entity = new CandidateCertificate
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = profileId,
                Name = dto.Name,
                Issuer = dto.Issuer,
                IssuedAt = dto.IssuedAt,
                ExpiryAt = dto.ExpiryAt,
                Link = dto.Link
            };

            var added = await _repo.AddCandidateCertificateAsync(entity);
            return Ok(new { success = true, data = added });
        }

        // [PUT] api/candidate-profile/{profileId}/certificates/{certificateId}
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPut("{profileId}/certificates/{certificateId}")]
        public async Task<IActionResult> UpdateCertificate([FromRoute] Guid profileId, [FromRoute] Guid certificateId, [FromBody] CandidateCertificateDto dto)
        {
            if (dto == null) return BadRequest();

            var entity = new CandidateCertificate
            {
                Id = certificateId,
                CandidateProfileId = profileId,
                Name = dto.Name,
                Issuer = dto.Issuer,
                IssuedAt = dto.IssuedAt,
                ExpiryAt = dto.ExpiryAt,
                Link = dto.Link
            };

            await _repo.UpdateCandidateCertificateAsync(entity);
            return Ok(new { success = true });
        }

        // [DELETE] api/candidate-profile/{profileId}/certificates/{certificateId}
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpDelete("{profileId}/certificates/{certificateId}")]
        public async Task<IActionResult> DeleteCertificate([FromRoute] Guid profileId, [FromRoute] Guid certificateId)
        {
            await _repo.DeleteCandidateCertificateAsync(certificateId);
            return Ok(new { success = true });
        }

    }
}
