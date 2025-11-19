using Intervu.Application.Interfaces.ExternalServices;
using System.IO;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices
{
    /// <summary>
    /// Temporary file service stub - replace with Firebase when credentials are configured
    /// </summary>
    public class TempFileService : IFileService
    {
        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            // Stub implementation
            return Task.FromResult(true);
        }

        public Task<string> UploadFileAsync(Stream stream, string fileName)
        {
            // Stub implementation - return placeholder URL
            return Task.FromResult($"temp://placeholder/{fileName}");
        }
    }
}
