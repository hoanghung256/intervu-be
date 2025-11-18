using Intervu.Application.Interfaces.ExternalServices;
using Microsoft.AspNetCore.Mvc;
using PayOS.Exceptions;
using PayOS.Models.Webhooks;

namespace Intervu.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IPaymentService _paymentService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCheckOutUrl()
        {
            string checkoutUrl = await _paymentService.CreatePaymentOrderAsync(null, 2000, "hello");
            return Ok(new { checkoutUrl });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpendOrder()
        {
            try
            {
                var result = await _paymentService.CreateSpendOrderAsync(2000, "NUKL", "970436", "1026869673");
                return Ok(result);
                //return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("payos-webhook-test")]
        public IActionResult VerifyOrder(Webhook payload)
        {
            Console.WriteLine($"Webhook run at {DateTime.Now}, status = {payload.Code}");
            return Ok(payload.Code);
        }

        [HttpGet("register")]
        public async Task<IActionResult> RegisterAsync()
        {
            try
            {
                await _paymentService.RegisterWebhooks();
                return Ok("Registered");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
