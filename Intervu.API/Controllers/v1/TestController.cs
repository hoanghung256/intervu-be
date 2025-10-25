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
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var fileName = $"{FolderName}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                using (var stream = file.OpenReadStream())
                {
                    await _fileService.UploadFileAsync(stream, fileName);
                }
                var fileUrl = $"https://storage.googleapis.com/{_bucketName}/{fileName}";
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
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
        public async Task<IActionResult> GetProfile([FromQuery] int id)
        {
            var profile = await _context.Users.FindAsync(id);

            return Ok(new
            {
                message = "success",
                data = profile
            });
        }
    }
}
