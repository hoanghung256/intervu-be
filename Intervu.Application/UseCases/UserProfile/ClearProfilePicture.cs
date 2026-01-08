using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.UserProfile
{
    public class ClearProfilePicture : IClearProfilePicture
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public ClearProfilePicture(IUserRepository userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        public async Task<bool> ExecuteAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                // attempt to delete file (ignore result but catch exceptions)
                try
                {
                    await _fileService.DeleteFileAsync(user.ProfilePicture);
                }
                catch
                {
                    // swallow exception to avoid blocking DB update
                }
            }

            return await _userRepository.ClearProfilePictureAsync(userId);
        }
    }
}
