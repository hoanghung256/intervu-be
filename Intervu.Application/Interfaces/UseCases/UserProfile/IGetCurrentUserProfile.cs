using Intervu.Application.DTOs.User;

namespace Intervu.Application.Interfaces.UseCases.UserProfile
{
    public interface IGetCurrentUserProfile
    {
        Task<CurrentUserProfileDto?> ExecuteAsync(Guid userId);
    }
}