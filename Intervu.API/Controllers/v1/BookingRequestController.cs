using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.Interfaces.UseCases.BookingRequest;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/booking-requests")]
    public class BookingRequestController : ControllerBase
    {
        private readonly ICreateExternalBookingRequest _createExternal;
        private readonly ICreateJDBookingRequest _createJD;
        private readonly IRespondToBookingRequest _respond;
        private readonly IGetBookingRequests _getList;
        private readonly IGetBookingRequestDetail _getDetail;
        private readonly IPayBookingRequest _pay;
        private readonly ICancelBookingRequest _cancel;

        public BookingRequestController(
            ICreateExternalBookingRequest createExternal,
            ICreateJDBookingRequest createJD,
            IRespondToBookingRequest respond,
            IGetBookingRequests getList,
            IGetBookingRequestDetail getDetail,
            IPayBookingRequest pay,
            ICancelBookingRequest cancel)
        {
            _createExternal = createExternal;
            _createJD = createJD;
            _respond = respond;
            _getList = getList;
            _getDetail = getDetail;
            _pay = pay;
            _cancel = cancel;
        }

        /// <summary>
        /// Flow B: Candidate requests a session outside the coach's available time ranges
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPost("external")]
        public async Task<IActionResult> CreateExternalBookingRequest([FromBody] CreateExternalBookingRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _createExternal.ExecuteAsync(userId, dto);
            return Ok(new
            {
                success = true,
                message = "External booking request created successfully",
                data = result
            });
        }

        /// <summary>
        /// Flow C: Candidate submits JD + CV for a multi-round interview plan
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPost("jd-interview")]
        public async Task<IActionResult> CreateJDBookingRequest([FromBody] CreateJDBookingRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _createJD.ExecuteAsync(userId, dto);
            return Ok(new
            {
                success = true,
                message = "JD multi-round booking request created successfully",
                data = result
            });
        }

        /// <summary>
        /// Coach accepts or rejects a pending booking request
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Interviewer)]
        [HttpPost("{id:guid}/respond")]
        public async Task<IActionResult> RespondToBookingRequest(Guid id, [FromBody] RespondToBookingRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _respond.ExecuteAsync(userId, id, dto);
            return Ok(new
            {
                success = true,
                message = dto.IsApproved
                    ? "Booking request accepted successfully"
                    : "Booking request rejected",
                data = result
            });
        }

        /// <summary>
        /// Get booking requests for the authenticated user (candidate or coach)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet]
        public async Task<IActionResult> GetMyBookingRequests([FromQuery] GetBookingRequestsFilterDto filter)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
            var isCoach = role == UserRole.Coach;

            var (items, totalCount) = await _getList.ExecuteAsync(userId, isCoach, filter);
            return Ok(new
            {
                success = true,
                message = "Booking requests retrieved successfully",
                data = new
                {
                    items,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize
                }
            });
        }

        /// <summary>
        /// Get a single booking request by ID (candidate or coach only)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.CandidateOrInterviewer)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingRequestDetail(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getDetail.ExecuteAsync(userId, id);
            return Ok(new
            {
                success = true,
                message = "Booking request retrieved successfully",
                data = result
            });
        }

        /// <summary>
        /// Candidate pays for an Accepted booking request via PayOS
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPost("{id:guid}/pay")]
        public async Task<IActionResult> PayBookingRequest(Guid id, [FromBody] PayBookingRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var checkoutUrl = await _pay.ExecuteAsync(userId, id, dto.ReturnUrl);
            return Ok(new
            {
                success = true,
                data = new
                {
                    isPaid = checkoutUrl == null,
                    checkOutUrl = checkoutUrl
                }
            });
        }

        /// <summary>
        /// Candidate cancels a Pending or Accepted booking request
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBookingRequest(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _cancel.ExecuteAsync(userId, id);
            return Ok(new
            {
                success = true,
                message = "Booking request cancelled successfully",
                data = result
            });
        }
    }
}
