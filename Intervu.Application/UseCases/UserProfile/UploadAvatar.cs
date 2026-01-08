using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.UserProfile
{
    public class UploadAvatar : IUploadAvatar
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public UploadAvatar(IUserRepository userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        public async Task<string?> ExecuteAsync(Guid userId, IFormFile avatarFile)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                try
                {
                    await _fileService.DeleteFileAsync(user.ProfilePicture);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to delete existing avatar.", ex);
                }
            }

            var objectName = $"avatars/{userId}{Path.GetExtension(avatarFile.FileName)}";
            using var stream = avatarFile.OpenReadStream();
            var fileUrl = await _fileService.UploadFileAsync(stream, objectName, avatarFile.ContentType);

            return await _userRepository.UpdateProfilePictureAsync(userId, fileUrl);
        }
    }
}
