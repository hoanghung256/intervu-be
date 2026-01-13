using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.UserProfile
{
    public interface IUploadAvatar
    {
        Task<string?> ExecuteAsync(Guid userId, IFormFile avatarFile);
    }
}
