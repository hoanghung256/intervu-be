using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Application.Interfaces.UseCases.Audit;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin")]
    public class AdminController : Controller
    {
        private readonly IGetDashboardStats _getDashboardStats;
        private readonly IGetAllUsersForAdmin _getAllUsers;
        private readonly IFilterUsersForAdmin _filterUsers;
        private readonly IGetAllCompaniesForAdmin _getAllCompanies;
        private readonly IGetAllPayments _getAllPayments;
        private readonly IGetAllFeedbacks _getAllFeedbacks;
        private readonly IGetAllCoachForAdmin _getAllCoach;
        private readonly ICreateUserForAdmin _createUserForAdmin;
        private readonly IGetUserByIdForAdmin _getUserByIdForAdmin;
        private readonly IUpdateUserForAdmin _updateUserForAdmin;
        private readonly IDeleteUserForAdmin _deleteUserForAdmin;
        private readonly IActivateUserForAdmin _activateUserForAdmin;
        private readonly IGetAuditLogs _getAuditLogs;
        private readonly IGetInterviewReports _getInterviewReports;
        private readonly IGetInterviewReportDetail _getInterviewReportDetail;
        private readonly IResolveInterviewReport _resolveInterviewReport;
        private readonly IGetAdminDashboardCharts _getAdminDashboardCharts;
        private readonly IGetTopCoachesLeaderboard _getTopCoachesLeaderboard;
        private readonly IGetNeedsAttentionQueue _getNeedsAttentionQueue;
        private readonly IGetAllTransactionsForAdmin _getAllTransactions;
        private readonly IGetPineconeIndexStats _getPineconeIndexStats;
        private readonly IGetAiServicesHealth _getAiServicesHealth;
        private readonly IGetAiConfiguration _getAiConfiguration;
        private readonly IAdminTriggerVectorSync _triggerVectorSync;

        public AdminController(
            IGetDashboardStats getDashboardStats,
            IGetAllUsersForAdmin getAllUsers,
            IFilterUsersForAdmin filterUsers,
            IGetAllCompaniesForAdmin getAllCompanies,
            IGetAllPayments getAllPayments,
            IGetAllFeedbacks getAllFeedbacks,
            IGetAllCoachForAdmin getAllCoach,
            ICreateUserForAdmin createUserForAdmin,
            IGetUserByIdForAdmin getUserByIdForAdmin,
            IUpdateUserForAdmin updateUserForAdmin,
            IDeleteUserForAdmin deleteUserForAdmin,
            IActivateUserForAdmin activateUserForAdmin,
            IGetAuditLogs getAuditLogs,
            IGetInterviewReports getInterviewReports,
            IGetInterviewReportDetail getInterviewReportDetail,
            IResolveInterviewReport resolveInterviewReport,
            IGetAdminDashboardCharts getAdminDashboardCharts,
            IGetTopCoachesLeaderboard getTopCoachesLeaderboard,
            IGetNeedsAttentionQueue getNeedsAttentionQueue,
            IGetAllTransactionsForAdmin getAllTransactions,
            IGetPineconeIndexStats getPineconeIndexStats,
            IGetAiServicesHealth getAiServicesHealth,
            IGetAiConfiguration getAiConfiguration,
            IAdminTriggerVectorSync triggerVectorSync)
        {
            _getDashboardStats = getDashboardStats;
            _getAllUsers = getAllUsers;
            _filterUsers = filterUsers;
            _getAllCompanies = getAllCompanies;
            _getAllPayments = getAllPayments;
            _getAllFeedbacks = getAllFeedbacks;
            _getAllCoach = getAllCoach;
            _createUserForAdmin = createUserForAdmin;
            _getUserByIdForAdmin = getUserByIdForAdmin;
            _updateUserForAdmin = updateUserForAdmin;
            _deleteUserForAdmin = deleteUserForAdmin;
            _activateUserForAdmin = activateUserForAdmin;
            _getAuditLogs = getAuditLogs;
            _getInterviewReports = getInterviewReports;
            _getInterviewReportDetail = getInterviewReportDetail;
            _resolveInterviewReport = resolveInterviewReport;
            _getAdminDashboardCharts = getAdminDashboardCharts;
            _getTopCoachesLeaderboard = getTopCoachesLeaderboard;
            _getNeedsAttentionQueue = getNeedsAttentionQueue;
            _getAllTransactions = getAllTransactions;
            _getPineconeIndexStats = getPineconeIndexStats;
            _getAiServicesHealth = getAiServicesHealth;
            _getAiConfiguration = getAiConfiguration;
            _triggerVectorSync = triggerVectorSync;
        }

        /// <summary>
        /// Get dashboard statistics summary
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _getDashboardStats.ExecuteAsync();
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get dashboard chart data (Revenue Trend & User Growth)
        /// </summary>
        [HttpGet("charts")]
        public async Task<IActionResult> GetDashboardCharts()
        {
            try
            {
                var (revenue, userGrowth) = await _getAdminDashboardCharts.ExecuteAsync();
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = new { revenue, userGrowth }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get top performing coaches leaderboard
        /// </summary>
        [HttpGet("top-coaches")]
        public async Task<IActionResult> GetTopCoaches([FromQuery] int count = 5)
        {
            try
            {
                var coaches = await _getTopCoachesLeaderboard.ExecuteAsync(count);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = coaches
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get operations queue of items needing attention
        /// </summary>
        [HttpGet("attention-queue")]
        public async Task<IActionResult> GetAttentionQueue()
        {
            try
            {
                var queue = await _getNeedsAttentionQueue.ExecuteAsync();
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = queue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _getAllUsers.ExecuteAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Filter users by role and search
        /// </summary>
        [HttpGet("users/filter")]
        public async Task<IActionResult> FilterUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] UserRole? role = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var users = await _filterUsers.ExecuteAsync(page, pageSize, role, search);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get all companies with pagination
        /// </summary>
        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var companies = await _getAllCompanies.ExecuteAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = companies
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get all transactions (Earnings, Refunds, Payouts) with pagination and filters
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetAllTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] TransactionType? type = null,
            [FromQuery] TransactionStatus? status = null)
        {
            try
            {
                var transactions = await _getAllTransactions.ExecuteAsync(page, pageSize, type, status);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = transactions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get all feedbacks with pagination
        /// </summary>
        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var feedbacks = await _getAllFeedbacks.ExecuteAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = feedbacks
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get all interviewers with pagination
        /// </summary>
        [HttpGet("interviewers")]
        public async Task<IActionResult> GetAllCoach([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var interviewers = await _getAllCoach.ExecuteAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = interviewers
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserDto request)
        {
            try
            {
                var result = await _createUserForAdmin.ExecuteAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "User created successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            try
            {
                var user = await _getUserByIdForAdmin.ExecuteAsync(id);
                
                if (user == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found",
                        data = (object?)null
                    });

                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] AdminCreateUserDto request)
        {
            try
            {
                var result = await _updateUserForAdmin.ExecuteAsync(id, request);
                return Ok(new
                {
                    success = true,
                    message = "User updated successfully",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Suspend user (soft-delete)
        /// </summary>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            try
            {
                var status = await _deleteUserForAdmin.ExecuteAsync(id);

                if (status == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found",
                        data = (object?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User marked as inactive",
                    data = new
                    {
                        userId = id,
                        status = status.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        [HttpPut("users/{id}/activate")]
        public async Task<IActionResult> ActivateUser([FromRoute] Guid id)
        {
            try
            {
                var success = await _activateUserForAdmin.ExecuteAsync(id);

                if (!success)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found or already active",
                        data = (object?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User has been activated successfully",
                    data = new
                    {
                        userId = id,
                        status = "Active"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get audit logs with pagination
        /// </summary>
        [HttpGet("audit-log")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var logs = await _getAuditLogs.ExecutePagedAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = logs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Get audit logs for a specific interview room (for room report modal)
        /// </summary>
        [HttpGet("room-reports/{roomId:guid}/audit-logs")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> GetAuditLogsByRoomId([FromRoute] Guid roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            try
            {
                var logs = await _getAuditLogs.ExecuteByRoomAsync(roomId, page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = logs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Admin gets interview room reports for management.
        /// </summary>
        [HttpGet("room-reports")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> GetRoomReports([FromQuery] InterviewReportFilterRequest filter)
        {
            try
            {
                var reports = await _getInterviewReports.ExecuteAsync(filter);

                var items = reports.Items.Select(r => new
                {
                    id = r.Id,
                    interviewRoomId = r.InterviewRoomId,
                    reporter = new
                    {
                        id = r.ReporterId,
                        fullName = r.ReporterName
                    },
                    reason = r.Reason,
                    details = r.Details,
                    expectTo = r.ExpectTo,
                    content = r.Details,
                    reportType = r.Reason, // Mapping for FE compatibility
                    status = (int)r.Status,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt
                });

                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = new
                    {
                        items,
                        reports.TotalItems,
                        reports.PageSize,
                        reports.CurrentPage
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Admin gets room report detail for a specific interview room.
        /// </summary>
        [HttpGet("room-reports/{roomId:guid}/detail")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> GetRoomReportDetail([FromRoute] Guid roomId)
        {
            try
            {
                var detail = await _getInterviewReportDetail.ExecuteAsync(roomId);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = detail
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }

        /// <summary>
        /// Resolve room report with refund options
        /// </summary>
        [HttpPost("resolve-room-report")]
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        public async Task<IActionResult> ResolveRoomReport([FromBody] ResolveRoomReportRequest request)
        {
            try
            {
                var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid.TryParse(adminIdStr, out Guid adminId);
                await _resolveInterviewReport.ExecuteAsync(request, adminId);
                return Ok(new
                {
                    success = true,
                    message = "Report resolved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ─── System Management ───────────────────────────────────────────────

        /// <summary>
        /// Get Pinecone index stats (vector counts per namespace)
        /// </summary>
        [HttpGet("system/pinecone-stats")]
        public async Task<IActionResult> GetPineconeStats()
        {
            try
            {
                var stats = await _getPineconeIndexStats.ExecuteAsync();
                return Ok(new { success = true, message = "Success", data = stats });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message, data = (object?)null });
            }
        }

        /// <summary>
        /// Trigger a full vector re-sync for the given namespace ("coaches" or "questions")
        /// </summary>
        [HttpPost("system/pinecone-sync")]
        public async Task<IActionResult> TriggerVectorSync([FromBody] TriggerSyncRequest request)
        {
            try
            {
                await _triggerVectorSync.ExecuteAsync(request.Namespace);
                return Ok(new { success = true, message = $"Sync job for '{request.Namespace}' has been queued." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get live health status of all configured AI services
        /// </summary>
        [HttpGet("system/ai-health")]
        public async Task<IActionResult> GetAiServicesHealth()
        {
            try
            {
                var health = await _getAiServicesHealth.ExecuteAsync();
                return Ok(new { success = true, message = "Success", data = health });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message, data = (object?)null });
            }
        }

        /// <summary>
        /// Get read-only AI service configuration (model names, endpoints, key presence)
        /// </summary>
        [HttpGet("system/ai-config")]
        public async Task<IActionResult> GetAiConfiguration()
        {
            try
            {
                var config = await _getAiConfiguration.ExecuteAsync();
                return Ok(new { success = true, message = "Success", data = config });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message, data = (object?)null });
            }
        }
    }

    public record TriggerSyncRequest(string Namespace);
}
