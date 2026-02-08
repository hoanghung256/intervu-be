using Intervu.Application.DTOs.Admin;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IUpdateUserForAdmin
    {
        Task<AdminUserResponseDto> ExecuteAsync(Guid userId, AdminCreateUserDto dto);
    }
}
