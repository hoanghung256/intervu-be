using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities.Constants;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IFilterUsersForAdmin
    {
        Task<PagedResult<UserDto>> ExecuteAsync(int page, int pageSize, UserRole? role, string? search);
    }
}
