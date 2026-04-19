using Asp.Versioning;
using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Industry;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetAllIndustries([FromQuery] PaginationParams @params)
        {
            var industries = await _getAllIndustries.ExecuteAsync(@params.Page, @params.PageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = industries
            });
        }
    }
}
