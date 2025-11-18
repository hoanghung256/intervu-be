using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.UserProfile
{
    public interface IUpdateProfilePicture
    {
        Task<string?> ExecuteAsync(int userId, IFormFile profilePicture);
    }
}
