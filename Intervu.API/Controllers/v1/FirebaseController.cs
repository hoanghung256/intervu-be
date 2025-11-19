using Google.Api.Gax.ResourceNames;
using Google.Cloud.Storage.V1;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Intervu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly string FolderName = "uploads";
        private readonly IFileService _fileService;
        private readonly IntervuDbContext _context;
        public FirebaseController(StorageClient storageClient, IFileService fileService, string bucketName, IntervuDbContext context)
        {
            _storageClient = storageClient;
            _fileService = fileService;
            _bucketName = bucketName;
            _context = context;
        }

        [HttpGet("get-avatar/{userId}")]
        public async Task<IActionResult> GetUserAvatar(int userId)
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
        public async Task<IActionResult> UploadUserAvatar(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("User not found.");

                var fileName = $"{FolderName}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                using (var stream = file.OpenReadStream())
                {
                    await _fileService.UploadFileAsync(stream, fileName);
                }

                var fileUrl = $"https://storage.googleapis.com/{_bucketName}/{fileName}";

                user.ProfilePicture = fileUrl;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    userId,
                    avatar = fileUrl,
                    message = "Avatar updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading avatar: {ex.Message}");
            }
        }

        [HttpDelete("delete-avatar/{userId}")]
        public async Task<IActionResult> DeleteUserAvatar(int userId)
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
