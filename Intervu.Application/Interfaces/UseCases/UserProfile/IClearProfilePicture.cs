using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.UserProfile
{
    public interface IClearProfilePicture
    {
        Task<bool> ExecuteAsync(Guid userId);
    }
}
