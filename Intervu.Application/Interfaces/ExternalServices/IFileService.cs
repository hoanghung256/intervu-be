using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream stream, string fileName, string contentType);
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}
