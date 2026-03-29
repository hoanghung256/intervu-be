using System;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.UseCases.Admin;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Admin
{
    public class ActivateUserForAdmin : IActivateUserForAdmin
    {
        private readonly IUserRepository _userRepository;

        public ActivateUserForAdmin(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> ExecuteAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (user.Status == UserStatus.Suspended)
            {
                user.Status = UserStatus.Active;
                _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
