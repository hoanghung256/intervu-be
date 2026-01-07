using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllCompaniesForAdmin
    {
        Task<PagedResult<CompanyDto>> ExecuteAsync(int page, int pageSize);
    }
}
