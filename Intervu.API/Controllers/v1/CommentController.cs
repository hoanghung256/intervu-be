using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.Comment;
using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/questions/{questionId:guid}/comments")]
    public class CommentController : ControllerBase
    {
        private readonly IGetComments _getComments;
        private readonly IAddComment _addComment;
        private readonly IUpdateComment _updateComment;
        private readonly IDeleteComment _deleteComment;
        private readonly ILikeComment _likeComment;

        public CommentController(
            IGetComments getComments,
            IAddComment addComment,
            IUpdateComment updateComment,
            IDeleteComment deleteComment,
            ILikeComment likeComment)
        {
            _getComments = getComments;
            _addComment = addComment;
            _updateComment = updateComment;
            _deleteComment = deleteComment;
            _likeComment = likeComment;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(
            Guid questionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortOption? sortBy = null)
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(raw, out var uid) ? uid : null;
            var result = await _getComments.ExecuteAsync(questionId, page, pageSize, sortBy, userId);
            return Ok(new { success = true, message = "Success", data = result });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost("{commentId:guid}/like")]
        public async Task<IActionResult> LikeComment(Guid commentId)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            var isLiked = await _likeComment.ExecuteAsync(commentId, userId);
            var isUpvote = isLiked;
            await _updateComment.VoteAsync(commentId, isUpvote, userId);
            return Ok(new { success = true, message = isLiked ? "Liked" : "Unliked", data = new { isLiked } });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPost]
        public async Task<IActionResult> AddComment(Guid questionId, [FromBody] CreateCommentRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            var id = await _addComment.ExecuteAsync(questionId, request, userId);
            return Ok(new { success = true, message = "Comment added", data = id });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpPut("{commentId:guid}")]
        public async Task<IActionResult> UpdateComment(Guid questionId, Guid commentId, [FromBody] UpdateCommentRequest request)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _updateComment.ExecuteAsync(commentId, request, userId);
            return Ok(new { success = true, message = "Comment updated" });
        }

        [Authorize(Policy = AuthorizationPolicies.AllRoles)]
        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid questionId, Guid commentId)
        {
            _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
            await _deleteComment.ExecuteAsync(commentId, userId);
            return Ok(new { success = true, message = "Comment deleted" });
        }

        //[Authorize(Policy = AuthorizationPolicies.AllRoles)]
        //[HttpPost("{commentId:guid}/vote")]
        //public async Task<IActionResult> Vote(Guid commentId, [FromQuery] bool isUpvote)
        //{
        //    _ = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);
        //    await _updateComment.VoteAsync(commentId, isUpvote, userId);
        //    return Ok(new { success = true, message = "Vote recorded" });
        //}
    }
}
