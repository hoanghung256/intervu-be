using Intervu.Application.DTOs.User;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.UserProfile
{
    public interface IChangePassword
    {
        Task<bool> ExecuteAsync(int userId, ChangePasswordRequest request);
    }
}
