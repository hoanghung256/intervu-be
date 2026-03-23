using Asp.Versioning;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/smart-search")]
    public class SmartSearchController : Controller
    {
        private readonly ISmartSearchCoach _smartSearchCoach;
        private readonly ISyncCoachVectors _syncCoachVectors;

        public SmartSearchController(
            ISmartSearchCoach smartSearchCoach,
            ISyncCoachVectors syncCoachVectors)
        {
            _smartSearchCoach = smartSearchCoach;
            _syncCoachVectors = syncCoachVectors;
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
        /// Sync all coach profiles to Pinecone vector store (Admin only)
        /// </summary>
        [HttpPost("sync-vectors")]
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
    }
}
