using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.UseCases.CoachDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1.Interviewer
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/coach/dashboard")]
    [Authorize(Policy = AuthorizationPolicies.Interviewer)]
    public class CoachDashboardController : ControllerBase
    {
        private readonly IGetCoachDashboardStats _getStats;
        private readonly IGetCoachUpcomingSessions _getSessions;
        private readonly IGetCoachPendingRequests _getRequests;
        private readonly IGetCoachServiceDistribution _getServices;
        private readonly IGetCoachFeedbackWall _getFeedbacks;
        private readonly IGetCoachAvailabilityOverview _getAvailability;

        public CoachDashboardController(
            IGetCoachDashboardStats getStats,
            IGetCoachUpcomingSessions getSessions,
            IGetCoachPendingRequests getRequests,
            IGetCoachServiceDistribution getServices,
            IGetCoachFeedbackWall getFeedbacks,
            IGetCoachAvailabilityOverview getAvailability)
        {
            _getStats = getStats;
            _getSessions = getSessions;
            _getRequests = getRequests;
            _getServices = getServices;
            _getFeedbacks = getFeedbacks;
            _getAvailability = getAvailability;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string period = "month")
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getStats.ExecuteAsync(userId, period);
            return Ok(new { success = true, message = "Dashboard stats retrieved", data = result });
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getSessions.ExecuteAsync(userId);
            return Ok(new { success = true, message = "Upcoming sessions retrieved", data = result });
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getRequests.ExecuteAsync(userId);
            return Ok(new { success = true, message = "Pending requests retrieved", data = result });
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getServices.ExecuteAsync(userId);
            return Ok(new { success = true, message = "Service distribution retrieved", data = result });
        }

        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetFeedbacks()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getFeedbacks.ExecuteAsync(userId);
            return Ok(new { success = true, message = "Feedback wall retrieved", data = result });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _getAvailability.ExecuteAsync(userId);
            return Ok(new { success = true, message = "Availability overview retrieved", data = result });
        }
    }
}
