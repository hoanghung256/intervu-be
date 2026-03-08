using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.Interfaces.UseCases.CoachInterviewService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/coach-interview-services")]
    public class CoachInterviewServiceController : ControllerBase
    {
        private readonly ICreateCoachInterviewService _createService;
        private readonly IUpdateCoachInterviewService _updateService;
        private readonly IDeleteCoachInterviewService _deleteService;
        private readonly IGetCoachInterviewServices _getServices;

        public CoachInterviewServiceController(
            ICreateCoachInterviewService createService,
            IUpdateCoachInterviewService updateService,
            IDeleteCoachInterviewService deleteService,
            IGetCoachInterviewServices getServices)
        {
            _createService = createService;
            _updateService = updateService;
            _deleteService = deleteService;
            _getServices = getServices;
        }

        /// <summary>
        /// Get all interview services offered by a coach
        /// </summary>
        [HttpGet("coach/{coachId:guid}")]
        public async Task<IActionResult> GetByCoachId(Guid coachId)
        {
            var result = await _getServices.ExecuteAsync(coachId);
            return Ok(new
            {
                success = true,
                message = "Coach interview services retrieved successfully",
                data = result
            });
        }

        /// <summary>
        /// Get all interview services for the authenticated coach
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyServices()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getServices.ExecuteAsync(userId);
            return Ok(new
            {
                success = true,
                message = "Coach interview services retrieved successfully",
                data = result
            });
        }

        /// <summary>
        /// Coach adds a new interview service they offer
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoachInterviewServiceDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _createService.ExecuteAsync(userId, dto);
            return Ok(new
            {
                success = true,
                message = "Coach interview service created successfully",
                data = result
            });
        }

        /// <summary>
        /// Coach updates an existing interview service (price, duration)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPut("{serviceId:guid}")]
        public async Task<IActionResult> Update(Guid serviceId, [FromBody] UpdateCoachInterviewServiceDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _updateService.ExecuteAsync(userId, serviceId, dto);
            return Ok(new
            {
                success = true,
                message = "Coach interview service updated successfully",
                data = result
            });
        }

        /// <summary>
        /// Coach removes an interview service they offer
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpDelete("{serviceId:guid}")]
        public async Task<IActionResult> Delete(Guid serviceId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _deleteService.ExecuteAsync(userId, serviceId);
            return Ok(new
            {
                success = true,
                message = "Coach interview service deleted successfully"
            });
        }
    }
}
