using Asp.Versioning;
using Intervu.API.Utils.Constant;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/smart-search")]
    public class SmartSearchController : Controller
    {
        private readonly ISmartSearchCoach _smartSearchCoach;
        private readonly ISmartSearchQuestion _smartSearchQuestion;
        private readonly ISyncCoachVectors _syncCoachVectors;
        private readonly ISyncQuestionVectors _syncQuestionVectors;

        public SmartSearchController(
            ISmartSearchCoach smartSearchCoach,
            ISmartSearchQuestion smartSearchQuestion,
            ISyncCoachVectors syncCoachVectors,
            ISyncQuestionVectors syncQuestionVectors)
        {
            _smartSearchCoach = smartSearchCoach;
            _smartSearchQuestion = smartSearchQuestion;
            _syncCoachVectors = syncCoachVectors;
            _syncQuestionVectors = syncQuestionVectors;
        }

        /// <summary>
        /// Smart search for coaches using natural language query
        /// </summary>
        [HttpPost("coaches")]
        public async Task<IActionResult> SearchCoaches([FromBody] SmartSearchRequest request)
        {
            try
            {
                var results = await _smartSearchCoach.ExecuteAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = results
                });
            }
            catch (ArgumentException ex)
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
        /// Smart search for question bank using natural language query
        /// </summary>
        [HttpPost("questions")]
        public async Task<IActionResult> SearchQuestions([FromBody] QuestionSmartSearchRequestDto request)
        {
            try
            {
                var results = await _smartSearchQuestion.ExecuteAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = results
                });
            }
            catch (ArgumentException ex)
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
        /// Sync all coach profiles to Pinecone vector store (Admin only)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPost("sync-vectors/coaches")]
        public async Task<IActionResult> SyncVectors()
        {
            try
            {
                var syncedCount = await _syncCoachVectors.ExecuteAsync();
                return Ok(new
                {
                    success = true,
                    message = $"Successfully synced {syncedCount} coach(es) to vector store.",
                    data = new { syncedCount }
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
        /// Sync all approved question bank entries to Pinecone vector store (Admin only)
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.Admin)]
        [HttpPost("sync-vectors/questions")]
        public async Task<IActionResult> SyncQuestionVectors()
        {
            try
            {
                var syncedCount = await _syncQuestionVectors.ExecuteAsync();
                return Ok(new
                {
                    success = true,
                    message = $"Successfully synced {syncedCount} question(s) to vector store.",
                    data = new { syncedCount }
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
    }
}
