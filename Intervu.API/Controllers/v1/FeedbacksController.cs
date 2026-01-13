using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.Interfaces.UseCases.Feedbacks;
using Intervu.Application.Interfaces.UseCases.InterviewRoom;
using Intervu.Domain.Entities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Intervu.API.Controllers.v1.InterviewRoomController;
using System.Security.Claims;
using Intervu.Application.DTOs.Feedback;
using System;
using Intervu.Domain.Entities;
using System.Threading.Tasks;
using Intervu.Domain.Repositories;

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

        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            bool isGetUserIdSuccess = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
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

        [Authorize(Policy = AuthorizationPolicies.Candidate)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback([FromRoute] Guid id, [FromBody] UpdateFeedbackDto updateFeedbackDto)
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
        public async Task<IActionResult> GetFeedbacksByInterviewRoom([FromRoute] Guid interviewRoomId)
        {
            try
            {
                var feedbacks = await _feedbackRepository.GetFeedbacksByInterviewRoomIdAsync(interviewRoomId);
                
                var result = new List<object>();
                foreach (var feedback in feedbacks)
                {
                    var student = await _userRepository.GetByIdAsync(feedback.CandidateId);
                    var coach = await _userRepository.GetByIdAsync(feedback.CoachId);

                    result.Add(new
                    {
                        id = feedback.Id,
                        interviewerId = feedback.CoachId,
                        studentId = feedback.CandidateId,
                        interviewRoomId = feedback.InterviewRoomId,
                        rating = feedback.Rating,
                        comments = feedback.Comments ?? "",
                        aiAnalysis = feedback.AIAnalysis ?? "",
                        studentName = student?.FullName ?? "Unknown",
                        studentEmail = student?.Email ?? "Unknown",
                        interviewerName = coach?.FullName ?? "Unknown",
                        interviewerEmail = coach?.Email ?? "Unknown"
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
