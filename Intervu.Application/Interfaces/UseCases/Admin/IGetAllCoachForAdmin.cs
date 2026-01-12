using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllCoachForAdmin
    {
        Task<PagedResult<CoachAdminDto>> ExecuteAsync(int page, int pageSize);
    }
}
