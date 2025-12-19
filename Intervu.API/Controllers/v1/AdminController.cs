using Asp.Versioning;
using Intervu.Application.Interfaces.UseCases.Admin;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IGetAllCompaniesForAdmin _getAllCompanies;
        private readonly IGetAllFeedbacks _getAllFeedbacks;
        private readonly IGetAllInterviewersForAdmin _getAllInterviewers;

        public AdminController(
            IGetDashboardStats getDashboardStats,
            IGetAllUsersForAdmin getAllUsers,
            IGetAllCompaniesForAdmin getAllCompanies,
            IGetAllFeedbacks getAllFeedbacks,
            IGetAllInterviewersForAdmin getAllInterviewers)
        {
            _getDashboardStats = getDashboardStats;
            _getAllUsers = getAllUsers;
            _getAllCompanies = getAllCompanies;
            _getAllFeedbacks = getAllFeedbacks;
            _getAllInterviewers = getAllInterviewers;
        }

        /// <summary>
        /// Get dashboard statistics summary
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _getDashboardStats.ExecuteAsync();
            return Ok(new
            {
                success = true,
                message = "Success",
                data = stats
            });
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _getAllUsers.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = users
            });
        }

        /// <summary>
        /// Get all companies with pagination
        /// </summary>
        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var companies = await _getAllCompanies.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = companies
            });
        }

        /// <summary>
        /// Get all payments with pagination
        /// </summary>
        //[HttpGet("payments")]
        //public async Task<IActionResult> GetAllPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    var payments = await _getAllPayments.ExecuteAsync(page, pageSize);
        //    return Ok(new
        //    {
        //        success = true,
        //        message = "Success",
        //        data = payments
        //    });
        //}

        /// <summary>
        /// Get all feedbacks with pagination
        /// </summary>
        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var feedbacks = await _getAllFeedbacks.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = feedbacks
            });
        }

        /// <summary>
        /// Get all interviewers with pagination
        /// </summary>
        [HttpGet("interviewers")]
        public async Task<IActionResult> GetAllInterviewers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var interviewers = await _getAllInterviewers.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = interviewers
            });
        }
    }
}
