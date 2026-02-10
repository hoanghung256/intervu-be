using Asp.Versioning;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Security.Claims;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Domain.Entities.Constants;

namespace Intervu.API.Controllers.v1.Payment
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/interview-booking")]
    public class InterviewBookingController : Controller
    {
        private readonly ILogger<InterviewBookingController> _logger;
        private readonly ICreateBookingCheckoutUrl _createBookingCheckoutUrl;
        private readonly IHandldeInterviewBookingUpdate _handldeInterviewBookingUpdate;
        private readonly IGetInterviewBooking _getInterviewBooking;

        public InterviewBookingController(
            ILogger<InterviewBookingController> logger,
            ICreateBookingCheckoutUrl createBookingCheckoutUrl,
            IGetInterviewBooking getInterviewBooking,
            IHandldeInterviewBookingUpdate handldeInterviewBookingUpdate)
        {
            _logger = logger;
            _createBookingCheckoutUrl = createBookingCheckoutUrl;
            _handldeInterviewBookingUpdate = handldeInterviewBookingUpdate;
            _getInterviewBooking = getInterviewBooking;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] InterviewBookingRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

            string? checkOutUrl = await _createBookingCheckoutUrl.ExecuteAsync(userId, request.CoachId, request.CoachAvailabilityId, request.ReturnUrl);

            return Ok(new
            {
                success = true,
                data = new
                {
                    isPaid = checkOutUrl == null,
                    checkOutUrl
                }
            });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> VerifyPaymentAsync(Webhook payload)
        {
            try
            {
                await _handldeInterviewBookingUpdate.ExecuteAsync(payload);

                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        //[HttpGet("register-webhook")]
        //public async Task<IActionResult> RegisterAsync()
        //{
        //    try
        //    {
        //        await _paymentService.RegisterWebhooks();
        //        return Ok("Registered");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("{orderCode}")]
        public async Task<IActionResult> GetTransaction([FromRoute] int orderCode)
        {
            InterviewBookingTransaction? t = await _getInterviewBooking.Get(orderCode, TransactionType.Payment);
            return Ok(new
            {
                success = true,
                data = t
            });
        }
    }
}
