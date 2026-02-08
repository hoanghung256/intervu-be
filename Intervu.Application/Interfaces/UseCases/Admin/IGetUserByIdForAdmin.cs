using Intervu.Application.DTOs.Admin;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetUserByIdForAdmin
    {
        Task<AdminUserResponseDto?> ExecuteAsync(Guid userId);
    }
}
