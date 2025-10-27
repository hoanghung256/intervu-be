using Asp.Versioning;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Storage.V1;
using Intervu.Application.ExternalServices;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Intervu.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    //[ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly IntervuDbContext _context;
        private readonly string FolderName = "uploads";
        private readonly IFileService _fileService;

        //public TestController(StorageClient storageClient, IFileService fileService, string bucketName, IntervuDbContext context)
        //{
        //    _storageClient = storageClient;
        //    _fileService = fileService;
        //    _bucketName = bucketName;
        //    _context = context;
        //}

        public TestController(IntervuDbContext context)
        {
            //_storageClient = storageClient;
            //_fileService = fileService;
            //_bucketName = bucketName;
            _context = context;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetFile()
        //{
        //    try
        //    {
        //        var images = new List<object>();
        //        await foreach (var obj in _storageClient.ListObjectsAsync(_bucketName, FolderName + "/"))
        //        {
        //            images.Add(new { name = obj.Name });
        //        }

        //        return Ok(images);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error listing images: {ex.Message}");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> UploadFile([FromBody] UserRequest request)
        {
            try
            {
                await _context.Users.AddAsync(new Domain.Entities.User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Password = request.Password,
                    Role = Domain.Entities.Constants.UserRole.Interviewee,
                    Status = Domain.Entities.Constants.UserStatus.Active,
                    ProfilePicture = null
                });
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {
                return Ok(new
                {
                    succes = false,
                    message = ex.Message,
                    data = ""
                });
            }

            return Ok(new
            {
                success = true,
                message = "Success",
                data = ""
            });
        }

        public class UserRequest
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("File name is required.");

            try
            {
                await _fileService.DeleteFileAsync(fileName);
                return Ok("File deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile([FromQuery] int id, [FromQuery] string alo)
        {
            var profile = await _context.Users.FindAsync(id);
            Console.WriteLine("check: " + alo);

            return Ok(new
            {
                success = true,
                message = "Success",
                data = profile
            });
        }
    }
}
