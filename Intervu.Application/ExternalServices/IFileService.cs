using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.ExternalServices
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName);
        Task DeleteFileAsync(string fileUrl);
    }
}
