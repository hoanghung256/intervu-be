using Intervu.Application.DTOs.User;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.UserProfile
{
    public interface IUpdateUserProfile
    {
        Task<UserDto?> ExecuteAsync(Guid userId, UpdateProfileRequest request);
    }
}
