using Azure.Core;
using Google.Cloud.Storage.V1;
using Intervu.Application.Interfaces.ExternalServices;
using Microsoft.AspNetCore.Http;

namespace Intervu.Infrastructure.ExternalServices.FirebaseStorageService
{
    public class FirebaseStorageService : IFileService
    {
        private readonly StorageClient _storage;
        private readonly string _bucketName;
        private readonly string FolderName = "uploads";
        private readonly string FirebaseBaseUrl = "https://firebasestorage.googleapis.com/v0/b/ntervu-4abd6.firebasestorage.app/o/";
        public FirebaseStorageService(StorageClient storage, string bucketName) 
        { 
            _storage = storage;
            _bucketName = bucketName;
        }
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileUrl));
            try
            {
                string objectName = fileUrl;
                if (!string.IsNullOrWhiteSpace(fileUrl) && fileUrl.StartsWith(FirebaseBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var remainder = fileUrl.Substring(FirebaseBaseUrl.Length);
                    var qIdx = remainder.IndexOf('?');
                    if (qIdx >= 0)
                        remainder = remainder.Substring(0, qIdx);
                    objectName = Uri.UnescapeDataString(remainder);
                }

                await _storage.DeleteObjectAsync(_bucketName, objectName);
                return true;
            }
            catch (Google.GoogleApiException gae) when (gae.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file: {ex.Message}", ex);
            }
        }

        //public async Task<string> UploadFileAsync(Stream stream, string objectName)
        //{
        //    try
        //    {
        //        var downloadToken = Guid.NewGuid().ToString();

        //        var storageObject = new Google.Apis.Storage.v1.Data.Object
        //        {
        //            Bucket = _bucketName,
        //            Name = objectName,
        //            ContentType = "image/png",
        //            Metadata = new Dictionary<string, string>
        //    {
        //        { "firebaseStorageDownloadTokens", downloadToken }
        //    }
        //        };


        //        var fileUrl = $"{FirebaseBaseUrl}{Uri.EscapeDataString(objectName)}?alt=media&token={downloadToken}";

        //        return fileUrl;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error uploading file: {ex.Message}", ex);
        //    }
        //}

        public async Task<string> UploadFileAsync(Stream stream, string objectName, string contentType)
        {
            try
            {
                var downloadToken = Guid.NewGuid().ToString();

                var storageObject = new Google.Apis.Storage.v1.Data.Object
                {
                    Bucket = _bucketName,
                    Name = objectName,
                    ContentType = contentType,
                    Metadata = new Dictionary<string, string>
        {
            { "firebaseStorageDownloadTokens", downloadToken }
        }
                };

                await _storage.UploadObjectAsync(storageObject, stream);

                var fileUrl = $"{FirebaseBaseUrl}{Uri.EscapeDataString(objectName)}?alt=media&token={downloadToken}";

                return fileUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}", ex);
            }
        }
    }
}
