using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Notification;
using Intervu.Application.Interfaces.UseCases.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationUseCase _notificationUseCase;

        public NotificationsController(INotificationUseCase notificationUseCase)
        {
            _notificationUseCase = notificationUseCase;
        }

        // --- User endpoints ---

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                return Unauthorized();

            var result = await _notificationUseCase.GetByUserIdAsync(userId, page, pageSize);

            return Ok(new { success = true, message = "Success", data = result });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                return Unauthorized();

            var count = await _notificationUseCase.GetUnreadCountAsync(userId);

            return Ok(new { success = true, message = "Success", data = new { count } });
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                return Unauthorized();

            await _notificationUseCase.MarkAsReadAsync(id, userId);

            return Ok(new { success = true, message = "Notification marked as read" });
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                return Unauthorized();

            await _notificationUseCase.MarkAllAsReadAsync(userId);

            return Ok(new { success = true, message = "All notifications marked as read" });
        }

        // --- Admin endpoints ---

        /// <summary>Send a notification to a specific user.</summary>
        [HttpPost("admin")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> AdminCreate([FromBody] CreateNotificationRequestDto request)
        {
            await _notificationUseCase.CreateAsync(
                request.UserId,
                request.Type,
                request.Title,
                request.Message,
                request.ActionUrl,
                request.ReferenceId);

            return Ok(new { success = true, message = "Notification sent" });
        }

        /// <summary>Broadcast a notification to multiple users (e.g. system announcement).</summary>
        [HttpPost("admin/broadcast")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> AdminBroadcast([FromBody] BroadcastNotificationRequestDto request)
        {
            if (request.UserIds == null || request.UserIds.Count == 0)
                return BadRequest(new { success = false, message = "UserIds must not be empty" });

            await _notificationUseCase.CreateForMultipleUsersAsync(
                request.UserIds,
                request.Type,
                request.Title,
                request.Message,
                request.ActionUrl);

            return Ok(new { success = true, message = $"Notification broadcast to {request.UserIds.Count} user(s)" });
        }

        /// <summary>Broadcast a notification to ALL users in the system.</summary>
        [HttpPost("admin/broadcast-all")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> AdminBroadcastAll([FromBody] BroadcastAllRequestDto request)
        {
            await _notificationUseCase.BroadcastToAllAsync(request.Type, request.Title, request.Message, request.ActionUrl);

            return Ok(new { success = true, message = "Notification broadcast to all users" });
        }

        /// <summary>Broadcast a notification to all users with a specific role.</summary>
        [HttpPost("admin/broadcast-role")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> AdminBroadcastRole([FromBody] BroadcastRoleRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new { success = false, message = "Role must not be empty" });

            await _notificationUseCase.BroadcastToRoleAsync(request.Role, request.Type, request.Title, request.Message, request.ActionUrl);

            return Ok(new { success = true, message = $"Notification broadcast to role '{request.Role}'" });
        }
    }
}