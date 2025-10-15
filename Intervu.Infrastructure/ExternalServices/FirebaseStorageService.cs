using Intervu.Application.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
