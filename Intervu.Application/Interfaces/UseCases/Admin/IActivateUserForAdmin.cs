using System;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IActivateUserForAdmin
    {
        Task<bool> ExecuteAsync(Guid userId);
    }
}
