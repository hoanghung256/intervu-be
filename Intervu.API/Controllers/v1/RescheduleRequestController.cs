using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.Interfaces.UseCases.RescheduleRequest;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reschedule-requests")]
    public class RescheduleRequestController : ControllerBase
    {
        private readonly ICreateRescheduleRequestUseCase _createRescheduleRequest;
        private readonly IRespondToRescheduleRequestUseCase _respondToRescheduleRequest;
        private readonly IRescheduleRequestRepository _rescheduleRequestRepository;

        public RescheduleRequestController(
            ICreateRescheduleRequestUseCase createRescheduleRequest,
            IRespondToRescheduleRequestUseCase respondToRescheduleRequest,
            IRescheduleRequestRepository rescheduleRequestRepository)
        {
            _createRescheduleRequest = createRescheduleRequest;
            _respondToRescheduleRequest = respondToRescheduleRequest;
            _rescheduleRequestRepository = rescheduleRequestRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRescheduleRequest([FromBody] CreateRescheduleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data"
                });
            }

            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            if (!isGetUserIdSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });
            }

            var requestId = await _createRescheduleRequest.ExecuteAsync(
                dto.RoomId,
                dto.ProposedAvailabilityId,
                userId,
                dto.Reason
            );

            return Ok(new
            {
                success = true,
                message = "Reschedule request created successfully",
                data = new { requestId }
            });
        }

        [Authorize]
        [HttpPost("{id}/respond")]
        public async Task<IActionResult> RespondToRescheduleRequest(Guid id, [FromBody] RespondToRescheduleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data"
                });
            }

            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid respondedBy);
            if (!isGetUserIdSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });
            }

            await _respondToRescheduleRequest.ExecuteAsync(id, respondedBy, dto.IsApproved, dto.RejectionReason);
            
            return Ok(new
            {
                success = true,
                message = "Responded to reschedule request successfully"
            });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRescheduleRequestById(Guid id)
        {
            var request = await _rescheduleRequestRepository.GetByIdWithDetailsAsync(id);
            
            if (request == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Reschedule request not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Success",
                data = request
            });
        }

        [Authorize]
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRescheduleRequests()
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            if (!isGetUserIdSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });
            }

            var requests = await _rescheduleRequestRepository.GetRescheduleRequestsByUserIdAsync(userId);
            
            return Ok(new
            {
                success = true,
                message = "Success",
                data = requests
            });
        }

        // Unused endpoint
        [Authorize]
        [HttpGet("pending-responses")]
        public async Task<IActionResult> GetPendingResponses()
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            if (!isGetUserIdSuccess)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });
            }

            var requests = await _rescheduleRequestRepository.GetPendingRequestsForResponderAsync(userId);
            
            return Ok(new
            {
                success = true,
                message = "Success",
                data = requests
            });
        }
    }
}
