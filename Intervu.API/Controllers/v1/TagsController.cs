using Asp.Versioning;
using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Tag;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TagsController : Controller
    {
        private readonly IGetAllTags _getAllTags;
        public TagsController(IGetAllTags getAllTags)
        {
            _getAllTags = getAllTags;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTags([FromQuery] PaginationParams @params)
        {
            var tags = await _getAllTags.ExecuteAsync(@params.Page, @params.PageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = tags
            });
        }
    }
}
