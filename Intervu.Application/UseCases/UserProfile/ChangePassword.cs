using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Application.Utils;
using Intervu.Domain.Repositories;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.UserProfile
{
    public class ChangePassword : IChangePassword
    {
        private readonly IUserRepository _userRepository;

        public ChangePassword(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> ExecuteAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return false;
            }

            // Verify current password
            if (!PasswordHashHandler.VerifyPassword(request.CurrentPassword, user.Password))
            {
                return false;
            }

            // Hash and update new password
            var hashedPassword = PasswordHashHandler.HashPassword(request.NewPassword);
            return await _userRepository.UpdatePasswordAsync(userId, hashedPassword);
        }
    }
}
