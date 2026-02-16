using Intervu.Application.DTOs.Admin;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface ICreateUserForAdmin
    {
        Task<AdminUserResponseDto> ExecuteAsync(AdminCreateUserDto dto);
    }
}
