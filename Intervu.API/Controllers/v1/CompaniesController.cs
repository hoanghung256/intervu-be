using Asp.Versioning;
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
        public async Task<IActionResult> GetAllCompanies([FromQuery] int page, [FromQuery] int pageSize)
        {
            var companies = await _getAllCompanies.ExecuteAsync(page, pageSize);
            return Ok(new
            {
                success = true,
                message = "Success",
                data = companies
            });
        }

    }
}
