using Asp.Versioning;
using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Company;
using Microsoft.AspNetCore.Mvc;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly IGetAllCompanies _getAllCompanies;
        public CompaniesController(IGetAllCompanies getAllCompanies)
        {
            _getAllCompanies = getAllCompanies;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies([FromQuery] PaginationParams @params)
        {
            var companies = await _getAllCompanies.ExecuteAsync(@params.Page, @params.PageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = companies
            });
        }

    }
}
