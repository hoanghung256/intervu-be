using Intervu.Application.Common;
using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllUsersForAdmin
    {
        Task<PagedResult<UserDto>> ExecuteAsync(int page, int pageSize);
    }
}
