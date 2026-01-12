using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Email;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.Email;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Intervu.Application.Interfaces.UseCases.Coach;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System;
using Intervu.Application.Interfaces.UseCases.Candidate;

namespace Intervu.API.Controllers.v1.Payment
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/interview-booking")]
    public class InterviewBookingController : Controller
    {
        private readonly ICreateBookingCheckoutUrl _createBookingCheckoutUrl;
        private readonly IPaymentService _paymentService;
        private readonly ICreateInterviewRoom _createInterviewRoom;
        private readonly IUpdateAvailabilityStatus _updateAvailabilityStatus;
        private readonly IUpdateBookingStatus _updateBookingStatus;
        private readonly IGetInterviewBooking _getInterviewBooking;
        private readonly ISendBookingConfirmationEmail _sendBookingConfirmationEmail;
        private readonly IGetCoachDetails _getCoachDetails;
        private readonly IGetCandidateDetails _getCandidateDetails;

        public InterviewBookingController(
            ICreateBookingCheckoutUrl createBookingCheckoutUrl,
            IPaymentService paymentService,
            ICreateInterviewRoom createInterviewRoom,
            IUpdateAvailabilityStatus updateAvailabilityStatus,
            IGetInterviewBooking getInterviewBooking,
            IUpdateBookingStatus updateBookingStatus,
            ISendBookingConfirmationEmail sendBookingConfirmationEmail,
            IGetCoachDetails getCoachDetails,
            IGetCandidateDetails getCandidateDetails)
        {
            _createBookingCheckoutUrl = createBookingCheckoutUrl;
            _paymentService = paymentService;
            _createInterviewRoom = createInterviewRoom;
            _updateAvailabilityStatus = updateAvailabilityStatus;
            _updateBookingStatus = updateBookingStatus;
            _getInterviewBooking = getInterviewBooking;
            _sendBookingConfirmationEmail = sendBookingConfirmationEmail;
            _getCoachDetails = getCoachDetails;
            _getCandidateDetails = getCandidateDetails;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] InterviewBookingRequest request)
        {
            try
            {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

            string checkOutUrl = await _createBookingCheckoutUrl.ExecuteAsync(userId, request.CoachId, request.CoachAvailabilityId, request.ReturnUrl);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        checkOutUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = "Create checkout url failed! Please try again"
                });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> VerifyPayment(Webhook payload)
        {
            try
            {
                bool isPaid = _paymentService.VerifyPaymentAsync(payload);
                return Ok(new { success = isPaid });
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpGet("register-webhook")]
        public async Task<IActionResult> RegisterAsync()
        {
            try
            {
                await _paymentService.RegisterWebhooks();
                return Ok("Registered");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction([FromRoute] Guid id)
        {
            InterviewBookingTransaction? t = await _getInterviewBooking.ExecuteAsync(id);
            return Ok(new
            {
                success = true,
                data = t
            });
        }
    }

    public class InterviewBookingRequest
    {
        public Guid CoachId { get; set; }
        public Guid CoachAvailabilityId { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;
    }
}
