using Asp.Versioning;
using Intervu.Application.DTOs.User;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.IntervieweeProfile;
using Intervu.Application.Interfaces.UseCases.UserProfile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Intervu.API.Controllers.v1.InterviewRoomController;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IGetUserProfile _getUserProfile;
        private readonly IUpdateUserProfile _updateUserProfile;
        private readonly IChangePassword _changePassword;
        private readonly IUpdateProfilePicture _updateProfilePicture;
        private readonly IFileService _fileService;
        private readonly IUpdateIntervieweeProfile _updateIntervieweeProfile;

        public UserProfileController(
            IGetUserProfile getUserProfile,
            IUpdateUserProfile updateUserProfile,
            IChangePassword changePassword,
            IUpdateProfilePicture updateProfilePicture,
            IFileService fileService,
            IUpdateIntervieweeProfile updateIntervieweeProfile)
        {
            _getUserProfile = getUserProfile;
            _updateUserProfile = updateUserProfile;
            _changePassword = changePassword;
            _updateProfilePicture = updateProfilePicture;
            _fileService = fileService;
            _updateIntervieweeProfile = updateIntervieweeProfile;
        }

        /// <summary>
        /// Get user profile by ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            var user = await _getUserProfile.ExecuteAsync(userId);
            
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Success",
                data = user
            });
        }

        /// <summary>
        /// Update user profile (full name)
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateProfileRequest request)
        {
            var user = await _updateUserProfile.ExecuteAsync(userId, request);
            
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Profile updated successfully",
                data = user
            });
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPut("{userId}/password")]
        public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request)
        {
            var success = await _changePassword.ExecuteAsync(userId, request);
            
            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Current password is incorrect or user not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Password changed successfully"
            });
        }

        /// <summary>
        /// Update profile picture
        /// </summary>
        [HttpPut("{userId}/profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture(Guid userId, IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Profile picture is required"
                });
            }

            var fileUrl = await _updateProfilePicture.ExecuteAsync(userId, profilePicture);
            
            if (fileUrl == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Profile picture updated successfully",
                data = new { profilePictureUrl = fileUrl }
            });
        }

        [HttpPost("upload-cv/{userId}")]
        public async Task<IActionResult> UploadCV(Guid userId, IFormFile file)
        {
            using var stream = file.OpenReadStream();

            string objectName = $"cvs/{userId}{Path.GetExtension(file.FileName)}";

            var fileUrl = await _fileService.UploadFileAsync(stream, objectName, file.ContentType);

            var profile = await _updateIntervieweeProfile.UpdateIntervieweeProfile(userId, fileUrl);
            if (profile == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                success = true,
                message = "Success",
                data = fileUrl
            });
        }
    }
}
