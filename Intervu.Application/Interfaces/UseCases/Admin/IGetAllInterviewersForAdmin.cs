using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Admin
{
    public interface IGetAllInterviewersForAdmin
    {
        Task<PagedResult<InterviewerAdminDto>> ExecuteAsync(int page, int pageSize);
    }
}
