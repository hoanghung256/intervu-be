using Asp.Versioning;
using Intervu.Application.Interfaces.UseCases.Industry;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class IndustriesController : ControllerBase
    {
        private readonly IGetAllIndustries _getAllIndustries;
        public IndustriesController(IGetAllIndustries getAllIndustries)
        {
            _getAllIndustries = getAllIndustries;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllIndustries([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            try
            {
                var industries = await _getAllIndustries.ExecuteAsync(page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    data = industries
                });
            }
            catch (Exception)
            {
                // Treat as normal case with 0 records if something goes wrong (e.g. table not migrated yet)
                return Ok(new
                {
                    success = true,
                    message = "Success (fallback)",
                    data = new { items = new List<object>(), totalItems = 0, currentPage = page, totalPages = 0 }
                });
            }
        }
    }
}
