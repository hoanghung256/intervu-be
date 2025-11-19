using System;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Email;
using Intervu.Application.Interfaces.UseCases.Email;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmailDemoController : ControllerBase
    {
        private readonly ISendBookingConfirmationEmail _sendBookingConfirmationEmailUseCase;

        public EmailDemoController(ISendBookingConfirmationEmail sendBookingConfirmationEmailUseCase)
        {
            _sendBookingConfirmationEmailUseCase = sendBookingConfirmationEmailUseCase;
        }

        /// <summary>
        /// Demo endpoint to send booking confirmation email
        /// </summary>
        [HttpPost("send-booking-confirmation")]
        public async Task<IActionResult> SendBookingConfirmationEmail([FromBody] SendBookingConfirmationEmailDto dto)
        {
            try
            {
                await _sendBookingConfirmationEmailUseCase.ExecuteAsync(dto);
                return Ok(new { message = "Email sent successfully", data = dto });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send email", details = ex.Message });
            }
        }

        /// <summary>
        /// Demo endpoint with sample data
        /// </summary>
        [HttpPost("send-booking-confirmation/sample")]
        public async Task<IActionResult> SendBookingConfirmationEmailSample()
        {
            try
            {
                var sampleDto = new SendBookingConfirmationEmailDto
                {
                    To = "anhnqde180064@fpt.edu.vn",
                    CandidateName = "Quoc Anh",
                    InterviewerName = "Chau Tinh Tri",
                    InterviewDate = DateTime.Now.AddDays(2),
                    InterviewTime = "14:00 UTC",
                    Position = "Senior Software Engineer",
                    Duration = 60,
                    BookingID = "BK-20251119-" + Guid.NewGuid().ToString().Substring(0, 8),
                    JoinLink = "https://intervu.com/room/abc123xyz",
                    RescheduleLink = "https://intervu.com/reschedule/abc123xyz"
                };

                await _sendBookingConfirmationEmailUseCase.ExecuteAsync(sampleDto);
                return Ok(new { message = "Sample email sent successfully", data = sampleDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send sample email", details = ex.Message });
            }
        }
    }
}
