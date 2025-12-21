using Asp.Versioning;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Storage.V1;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System;

namespace Intervu.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FirebaseController : ControllerBase
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly string FolderName = "uploads";
        private readonly IFileService _fileService;
        private readonly IntervuDbContext _context;
        private readonly string FirebaseBaseUrl = "https://firebasestorage.googleapis.com/v0/b/ntervu-4abd6.firebasestorage.app/o/";
        public FirebaseController(StorageClient storageClient, IFileService fileService, string bucketName, IntervuDbContext context)
        {
            _storageClient = storageClient;
            _fileService = fileService;
            _bucketName = bucketName;
            _context = context;
        }

        [HttpGet("get-avatar/{userId}")]
        public async Task<IActionResult> GetUserAvatar(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                userId,
                avatar = user.ProfilePicture
            });
        }

        [HttpPost("upload-avatar/{userId}")]
        public async Task<IActionResult> UploadUserAvatar(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("User not found.");

                string objectNameWithToken;
                using (var stream = file.OpenReadStream())
                {
                    objectNameWithToken = await _fileService.UploadFileAsync(stream, file.FileName);
                }
                var parts = objectNameWithToken.Split('|');
                var objectName = parts[0];
                var token = parts[1];

                var fileUrl = $"{FirebaseBaseUrl}{Uri.EscapeDataString(objectName)}?alt=media&token={token}";
                user.ProfilePicture = fileUrl;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        userId,
                        avatar = fileUrl,
                    },
                    message = "Avatar updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading avatar: {ex.Message}");
            }
        }



        [HttpDelete("delete-avatar/{userId}")]
        public async Task<IActionResult> DeleteUserAvatar(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            if (string.IsNullOrWhiteSpace(user.ProfilePicture))
                return BadRequest("User has no avatar to delete.");

            string fileName = user.ProfilePicture
                .Replace($"https://storage.googleapis.com/{_bucketName}/", "");

            await _fileService.DeleteFileAsync(fileName);

            user.ProfilePicture = null;
            await _context.SaveChangesAsync();

            return Ok("Avatar deleted successfully.");
        }


    }
}
