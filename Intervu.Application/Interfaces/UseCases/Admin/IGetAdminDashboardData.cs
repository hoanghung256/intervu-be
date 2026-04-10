using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAdminDashboardData
    {
        Task<AdminDashboardDataDto> ExecuteAsync();
    }
}
