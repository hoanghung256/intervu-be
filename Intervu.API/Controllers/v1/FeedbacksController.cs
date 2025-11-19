using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Intervu.API.Controllers.v1.InterviewRoomController;
using System.Security.Claims;
using Intervu.Application.DTOs.Feedback;
using Intervu.Domain.Entities;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FeedbacksController : ControllerBase
    {
        private readonly IGetFeedbacks _getFeedbacks;
        private readonly IUpdateFeedback _updateFeedback;
        private readonly ICreateFeedback _createFeedback;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUserRepository _userRepository;

        public FeedbacksController(
            IGetFeedbacks getFeedbacks, 
            IUpdateFeedback updateFeedback, 
            ICreateFeedback createFeedback,
            IFeedbackRepository feedbackRepository,
            IUserRepository userRepository)
        {
            _getFeedbacks = getFeedbacks;
            _updateFeedback = updateFeedback;
            _createFeedback = createFeedback;
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewee)]
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            bool isGetUserIdSuccess = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            bool isGetRoleSuccess = Enum.TryParse<UserRole>(User.FindFirstValue(ClaimTypes.Role), out UserRole role);

            GetFeedbackRequest request = new GetFeedbackRequest
            {
                StudentId = userId,
                Page = page,
                PageSize = pageSize,
            };
            var list = await _getFeedbacks.ExecuteAsync(request);

            return Ok(new
            {
                success = true,
                message = "Success",
                data = list
            });
        }

        [Authorize(Policy = AuthorizationPolicies.Interviewee)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback([FromRoute] int id, [FromBody] UpdateFeedbackDto updateFeedbackDto)
        {
            if (string.IsNullOrEmpty(updateFeedbackDto.Comments))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please input comment"
                });
            }
            if (updateFeedbackDto.Rating == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please input rating"
                });
            }
            Feedback? feedback = await _getFeedbacks.ExecuteAsync(id);
            if (!string.IsNullOrEmpty(feedback?.Comments))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "You have already done feedback this user"
                });
            }
            feedback.Rating = updateFeedbackDto.Rating;
            feedback.Comments = updateFeedbackDto.Comments;

            await _updateFeedback.ExecuteAsync(feedback);

            return Ok(new
            {
                success = true,
                message = "Feedback user successful",
            });
        }

        [Authorize(Policy = AuthorizationPolicies.InterviewOrAdmin)]
        [HttpGet("interview-room/{interviewRoomId}")]
        public async Task<IActionResult> GetFeedbacksByInterviewRoom([FromRoute] int interviewRoomId)
        {
            try
            {
                var feedbacks = await _feedbackRepository.GetFeedbacksByInterviewRoomIdAsync(interviewRoomId);
                
                var result = new List<object>();
                foreach (var feedback in feedbacks)
                {
                    var student = await _userRepository.GetByIdAsync(feedback.StudentId);
                    var interviewer = await _userRepository.GetByIdAsync(feedback.InterviewerId);

                    result.Add(new
                    {
                        id = feedback.Id,
                        interviewerId = feedback.InterviewerId,
                        studentId = feedback.StudentId,
                        interviewRoomId = feedback.InterviewRoomId,
                        rating = feedback.Rating,
                        comments = feedback.Comments ?? "",
                        aiAnalysis = feedback.AIAnalysis ?? "",
                        studentName = student?.FullName ?? "Unknown",
                        studentEmail = student?.Email ?? "Unknown",
                        interviewerName = interviewer?.FullName ?? "Unknown",
                        interviewerEmail = interviewer?.Email ?? "Unknown"
                    });
                }
                
                return Ok(new
                {
                    success = true,
                    message = "Feedbacks retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

#if DEBUG
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Feedback feedback)
        {
            await _createFeedback.ExecuteAsync(feedback);
            return Ok(new
            {
                success = true,
                message = "Feedback successfully created",
                data = feedback,
            });
        }
#endif
    }
}
