using Intervu.Application.ExternalServices;

namespace Intervu.Infrastructure.ExternalServices
{
    public class FirebaseStorageService : IFileService
    {
        public Task DeleteFileAsync(string fileUrl)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
