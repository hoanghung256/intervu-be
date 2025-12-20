using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Intervu.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.UserProfile
{
    public class UpdateProfilePicture : IUpdateProfilePicture
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public UpdateProfilePicture(IUserRepository userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        public async Task<string?> ExecuteAsync(int userId, IFormFile profilePicture)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return null;
            }

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                await _fileService.DeleteFileAsync(user.ProfilePicture);
            }

            // Upload new profile picture
            var fileName = $"profile_{userId}_{Guid.NewGuid()}{Path.GetExtension(profilePicture.FileName)}";
            
            using var stream = profilePicture.OpenReadStream();
            var fileUrl = await _fileService.UploadFileAsync(stream, fileName);

            // Update user profile picture in database
            return await _userRepository.UpdateProfilePictureAsync(userId, fileUrl);
        }
    }
}
