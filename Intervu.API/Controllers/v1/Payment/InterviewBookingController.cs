using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.Availability;
using Intervu.Application.Interfaces.UseCases.InterviewBooking;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.UseCases.InterviewBooking;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Security.Claims;

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

        public InterviewBookingController(
            ICreateBookingCheckoutUrl createBookingCheckoutUrl,
            IPaymentService paymentService,
            ICreateInterviewRoom createInterviewRoom,
            IUpdateAvailabilityStatus updateAvailabilityStatus,
            IGetInterviewBooking getInterviewBooking,
            IUpdateBookingStatus updateBookingStatus)
        {
            _createBookingCheckoutUrl = createBookingCheckoutUrl;
            _paymentService = paymentService;
            _createInterviewRoom = createInterviewRoom;
            _updateAvailabilityStatus = updateAvailabilityStatus;
            _updateBookingStatus = updateBookingStatus;
            _getInterviewBooking = getInterviewBooking;
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.Interviewee)]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] InterviewBookingRequest request)
        {
            try
            {
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

                string checkOutUrl = await _createBookingCheckoutUrl.ExecuteAsync(userId, request.InterviewerId, request.InterviewerAvailabilityId, request.ReturnUrl);

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
                int orderId = (int)payload.Data.OrderCode;

                if (isPaid)
                {
                    // Update booking status if isPaid = true
                    Domain.Entities.InterviewBookingTransaction transaction = await _updateBookingStatus.ExecuteAsync(orderId, TransactionStatus.Paid);

                    // Update interviewer availability to booked
                    Domain.Entities.InterviewerAvailability avai = await _updateAvailabilityStatus.ExecuteAsync(transaction.InterviewerAvailabilityId, true);

                    // Create intervew room
                    int intervieweeId = transaction.UserId;
                    int interviewerId = avai.InterviewerId;
                    await _createInterviewRoom.ExecuteAsync(intervieweeId, interviewerId, avai.StartTime);

                    // TODO: Send notification to interviewer and interviewee (Mail/System Notification)
                }
                return Ok();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

        [HttpGet("{orderCode}")]
        // Note that orderCode of PayOS is InterviewBookingTransaction.Id in DB
        public async Task<IActionResult> GetTransaction([FromRoute] int orderCode)
        {
            InterviewBookingTransaction? t = await _getInterviewBooking.ExecuteAsync(orderCode);
            return Ok(new
            {
                success = true,
                data = t
            });
        }
    }

    public class InterviewBookingRequest
    {
        public int InterviewerId { get; set; }
        public int InterviewerAvailabilityId { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;
    }
}
