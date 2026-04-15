using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Withdrawal;
using Intervu.Application.Interfaces.UseCases.Withdrawal;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wallet")]
    [Authorize(Policy = AuthorizationPolicies.Interviewer)]
    public class WalletController : ControllerBase
    {
        private readonly ICoachProfileRepository _coachProfileRepository;
        private readonly IRequestWithdrawal _requestWithdrawal;
        private readonly IGetWithdrawalHistory _getWithdrawalHistory;

        public WalletController(
            ICoachProfileRepository coachProfileRepository,
            IRequestWithdrawal requestWithdrawal,
            IGetWithdrawalHistory getWithdrawalHistory)
        {
            _coachProfileRepository = coachProfileRepository;
            _requestWithdrawal = requestWithdrawal;
            _getWithdrawalHistory = getWithdrawalHistory;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var coach = await _coachProfileRepository.GetProfileByIdAsync(userId);

            if (coach == null)
                return NotFound(new { success = false, message = "Coach profile not found." });

            return Ok(new { success = true, data = new { balance = coach.CurrentAmount ?? 0 } });
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> RequestWithdrawal([FromBody] RequestWithdrawalDto request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _requestWithdrawal.ExecuteAsync(userId, request);
            return Ok(new { success = true, message = "Withdrawal request created successfully", data = result });
        }

        [HttpGet("withdrawals")]
        public async Task<IActionResult> GetWithdrawalHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (items, totalCount) = await _getWithdrawalHistory.ExecuteAsync(userId, page, pageSize);
            return Ok(new { success = true, data = new { items, totalCount } });
        }
    }
}
